# Job engine & status UI — design

Design for [roadmap step 2](../../roadmap/step-2-job-engine.md). Builds the single orchestration
mechanism every later pipeline stage rides on: a durable job table, an in-process executor, startup
recovery, and a live status page. No later stage grows its own progress, retry or status handling.

## Decisions

Four questions the step document left open, and what was chosen:

| Question | Decision | Why |
|---|---|---|
| What does the import page do after the retrofit? | Upload, enqueue, link to the jobs page. No inline unpack progress. | The jobs page is the one place work is watched. Duplicating a live view on the import page would mean maintaining two. |
| How much of the staleness mechanic lands now? | Registry and `(componentId, componentVersion)` stamping now; the invalidation sweep in step 3. | Unpack is not a stale-able unit — re-unpacking yields byte-identical blobs — so there is nothing here to bump and prove a sweep against. Stamping now means step 3 adds a query, not a migration. |
| Does a failed job retry itself? | No. One attempt, then it parks in `failed` with its error. Manual retry from the UI. | Nothing retries a deterministic bug usefully. A parked job with a visible error is information; three identical failures are noise. |
| Where does the queue live? | Postgres. The bounded channel is a dispatch buffer downstream of it. | Step 3 enqueues one job per catalog entry — hundreds of thousands. A channel-as-queue caps the backlog at the channel's capacity and makes enqueue block on its own queue. |

## Data model

One new table, `jobs`, in a new EF migration alongside the step 1 catalog tables. Table name snake_case, columns left as their property names — matching the existing
configurations, which map `ToTable` and nothing else.

| Column | Type | Notes |
|---|---|---|
| `id` | `uuid` | v7, as `ImportPackage` already uses |
| `kind` | `text` | `unpack` today; `classify`, `extract` later |
| `target_key` | `text` | the work unit's natural key — archive hash for unpack, catalog hash for step 3 |
| `component_id` | `text` | from the registry |
| `component_version` | `integer` | from the registry, stamped at enqueue |
| `state` | `text` | `Pending \| Running \| Succeeded \| Failed \| Stale`; a `JobState` enum converted to its name, so the column stays readable in psql and is not reordered by an enum edit |
| `attempts` | `integer` | incremented when execution starts |
| `payload` | `jsonb` | kind-specific; for unpack the staged path, file name and size |
| `error` | `text` | nullable |
| `progress_current` | `integer` | nullable |
| `progress_total` | `integer` | nullable |
| `progress_note` | `text` | nullable — the current file, for unpack |
| `enqueued_utc` | `timestamptz` | required |
| `started_utc` | `timestamptz` | nullable |
| `finished_utc` | `timestamptz` | nullable |

### One row per work unit

Unique index on `(kind, target_key)`. Enqueueing a unit that already exists upserts it back to
`pending` rather than appending a row.

This is what the spec means by a unit recording the version that last succeeded. It makes step 3's
sweep a single statement — `UPDATE jobs SET "State" = 'Stale' WHERE "ComponentId" = @id AND
"ComponentVersion" < @version` — and it bounds the table by work-unit count instead of run count.
Step 3 will hold roughly one row per catalog entry; per-run rows would grow without limit across
replays.

The cost is that per-run history is not kept: a job shows its last outcome only. Nothing in steps
3–7 asks for more. If an audit trail is ever wanted, a separate `job_runs` table can be added
without disturbing this one.

### Indexes

- unique `(kind, target_key)` — the upsert target
- `(state, enqueued_utc)` — the pump's claim query
- `(component_id, component_version)` — step 3's sweep

### Progress is persisted, not only pushed

A 40,000-file unpack needs progress to survive a restart, but writing 40,000 `UPDATE`s would cost
more than the unpack itself. Progress is written at most every `ProgressInterval` (default 500 ms)
and always on a state transition.

## Components

### Registry

```csharp
public interface IJobHandler
{
    string Kind { get; }
    string ComponentId { get; }
    int Version { get; }

    Task ExecuteAsync(JobContext context, CancellationToken cancellationToken);
}
```

Implementations are discovered from DI. `JobRegistry` indexes them by `Kind`, rejects a duplicate
kind at startup rather than picking one arbitrarily, and is what stamps `(component_id,
component_version)` at enqueue. Step 2 registers exactly one handler.

`JobContext` gives the handler its job row, its deserialized payload, and an `IProgress<JobProgress>`
that writes through the throttle described above.

The registry also exposes every registered handler, which is the surface step 3's invalidation sweep
will read to compare declared versions against stored ones. That sweep is not written here.

### Queue

`IJobQueue.EnqueueAsync(kind, targetKey, payload)` resolves the handler through the registry, upserts
the row to `pending` with the stamped version and a cleared error, and signals the pump. The insert
and the signal are ordered: the row is committed before the signal, so a signal never arrives for a
row that is not yet visible.

### Executor

`JobRunner : BackgroundService`, one pump and N workers.

**Pump.** Claims up to a batch of `pending` rows with `SELECT … FOR UPDATE SKIP LOCKED`, and in the
same transaction sets `state = running`, `started_utc`, and `attempts + 1`. Claimed ids go into the
bounded `Channel<Guid>`. The pump wakes on the queue's signal or on `PollInterval`, so a missed
signal costs latency rather than a stuck queue.

The channel holds `Parallelism * 2` ids and the claim batch is the channel's free capacity, so the
pump claims only what it can immediately hand over. Claiming more would flip rows to `running` while
they sit in a buffer, which is a lie the jobs page would display and startup recovery would have to
undo.

`SKIP LOCKED` is what makes the claim correct rather than merely convenient: it is the reason two
pumps — or a pump running while a leftover process is shutting down — can never hand the same row to
two workers.

**Workers.** Read an id, open a DI scope so each job gets a short-lived `StriveContext` (the same
reason `ImportPage` opens a scope per import today), resolve the handler by kind, execute, write the
terminal state.

**Options**, bound to a `Jobs:` configuration section:

| Setting | Default | What |
|---|---|---|
| `Jobs:Enabled` | `true` | Whether the runner starts at all |
| `Jobs:Parallelism` | `min(ProcessorCount, 8)` | Concurrent workers |
| `Jobs:PollInterval` | 5 s | Pump wake-up when no signal arrives |
| `Jobs:ProgressInterval` | 500 ms | Floor between persisted progress writes |

`Jobs:Enabled` exists because `StriveAppFactory` boots the real host: without it the runner would
start inside every existing integration test, and the kill-mid-run test would race a runner it did
not start.

### Startup recovery

Before the pump starts:

```sql
UPDATE jobs SET "State" = 'Pending', "StartedUtc" = NULL WHERE "State" IN ('Running', 'Stale')
```

The recovered count is logged. Only one instance runs, so a `running` row at startup is by
definition interrupted. `attempts` is deliberately untouched — interruption is not failure, and
consuming the single attempt the retry policy allows would turn a restart into a permanent failure.

`stale` is included even though nothing produces it yet, so step 3 inherits a recovery path that
already handles it.

## Failure

A handler throwing writes `state = failed`, the exception message into `error`, and `finished_utc`.
The job parks there. Manual retry from the UI resets it to `pending` and clears the error.

`OperationCanceledException` raised by the host's shutdown token is not a failure. It resets the job
to `pending` so it resumes on the next start, which is the same path startup recovery takes for a
hard kill.

## Idempotency

The job's terminal-state write and the importer's transaction are separate commits, so a crash
between them is possible. It is safe in both directions:

- **Crash before the import commits.** Recovery re-runs the job from scratch. Blobs already written
  deduplicate on write, so the retry costs time and no correctness.
- **Crash after the import commits, before the job is marked succeeded.** Recovery re-runs the job;
  `PackageImporter` finds the archive hash already present and returns `DuplicateArchive` without
  doing work.

The existing duplicate-archive check is what makes the unpack handler idempotent. No second
mechanism is introduced for it.

## Import retrofit

Staging stays on the circuit — the bytes come from the browser — and the upload progress bar stays
with it. When staging completes, the page enqueues an `unpack` job keyed by the archive hash and
reports "queued", linking to the jobs page.

The page no longer discards the staged ZIP. The handler owns that file: it deletes it once the
import commits or is found to be a duplicate, and **keeps it when the job fails**, so a manual retry
has something to read. A permanently failed job therefore leaves a file in `incoming/`. That is a
documented cost rather than a bug, and it is the same trade the blob store already makes with
unreferenced blobs.

## UI

A jobs page at `/jobs`, added to the nav menu.

- State counts across the top: pending, running, succeeded, failed.
- A table: kind, target, state, progress, duration, error, retry action.

Live updates come from a singleton `IJobNotifier`. The runner signals it on every state transition
and on each throttled progress tick. The notification carries only the fact that something changed;
the page re-reads through `IJobReader` on a ~200 ms throttle, reusing the render-throttle pattern
`ImportPage` already has.

Pushing job state through the notification itself would create two representations of the same row
that can disagree after a dropped or reordered message. A re-read cannot.

Subscriptions are taken in `OnInitializedAsync` and released in `Dispose`. Publication is
fire-and-forget with per-subscriber error isolation, so a faulted circuit cannot stall the runner —
the UI is a view of the job table, never the owner of the work.

## Testing

**Unit** (`Fip.Strive.Application.UnitTests`): registry resolution and duplicate-kind rejection,
progress throttling, options defaults.

**Integration** (`Fip.Strive.IntegrationTests`, real Postgres via Testcontainers):

- an enqueued job runs and reaches `succeeded`
- enqueueing one `(kind, target_key)` twice leaves one row
- concurrent claims never hand the same job to two workers
- a seeded `running` row is recovered on startup and completes
- a failed job parks, and manual retry re-runs it
- **the done criterion**: stage a real multi-file archive, kill the host mid-unpack, restart against
  the same database and data directory, and assert exactly one `ImportPackage` with the correct file
  count and no duplicate catalog entries

## Layout

```
Application/Features/Jobs/JobOptions.cs
Application/Features/Jobs/Models/{Job,JobState,JobProgress,JobContext,JobView}.cs
Application/Features/Jobs/Services/Contracts/{IJobQueue,IJobHandler,IJobRegistry,IJobNotifier,IJobReader}.cs
Application/Features/Jobs/Services/{JobQueue,JobRegistry,JobNotifier,JobReader,JobRunner}.cs
Application/Features/Import/Services/UnpackJobHandler.cs
Application/Infrastructure/Configurations/JobConfiguration.cs
Application/Infrastructure/Migrations/<timestamp>_Jobs.cs
Web/Components/Pages/JobsPage.razor(.cs)
```

`UnpackJobHandler` lives under `Import` rather than `Jobs`: it is import behaviour that implements a
jobs contract, which keeps the `Jobs` feature free of any knowledge about archives.

`Fip.Strive.Application` gains a `Microsoft.Extensions.Hosting.Abstractions` package reference for
`BackgroundService`. Abstractions only, so the layer stays host-agnostic and unit-testable.

`Registration.AddApplication` gains an `AddJobs` call registering the options, the registry, the
queue, the notifier, the reader, the discovered handlers, and the hosted runner.

## Out of scope

- Distributed execution, external queues, Hangfire or Quartz — as the step document states, revisit
  only if in-process proves insufficient.
- The staleness invalidation sweep (step 3).
- Per-run job history.
- Cleanup of staged archives belonging to permanently failed jobs.
