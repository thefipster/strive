# Step 2 — Job engine & status UI

## Goal

Build the single orchestration mechanism every later pipeline stage rides on: a persistent job table, an in-process background worker, and a live status UI. No stage ever grows its own ad-hoc progress/retry/status handling.

## Design

- **Job table** (Postgres): `(id, kind, targetKey, componentId, componentVersion, state, attempts, error, enqueued/started/finished timings)` with `state ∈ pending | running | succeeded | failed | stale`.
- **Work units are keyed and versioned**: e.g. a later parse unit is `(catalogEntryId, extractorId)` and records the version that last succeeded. A component version bump marks its units `stale`, which re-queues them — this is the backbone of self-healing.
- **Execution**: a hosted `BackgroundService` consuming a bounded `Channel<Job>`, parallelism tuned to the host (8 threads). Everything runs in-process in the Blazor Server app.
- **Never tied to the SignalR circuit**: closing the browser tab must not kill a run. The UI is a *view* of the job table, not the owner of the work.
- **Resumable**: on startup, `running` (interrupted) and `stale` jobs are re-enqueued.
- Retrofit step 1's unpack/hash work as the first real job kind.

## UI

- Jobs page: live view of the job table — pending/running/failed counts, per-job state, durations, errors, retry action.
- This page is the validation surface for every later step.

## Tasks

- [x] Job table schema + repository
- [x] `BackgroundService` + bounded channel executor with configurable parallelism
- [x] Startup recovery (re-enqueue `running`/`stale`)
- [~] Staleness mechanic: component registry with versions **(registry and version stamping only;
  the invalidation sweep moves to step 3)**
- [x] Live jobs page (server push updates)
- [x] Retrofit unpacking as a job kind

## Done criterion

Start a large unpack run, **kill the app mid-run, restart** — the run resumes and completes with no duplicate or lost work. Closing the browser tab during a run has no effect on it. Job progress and errors are visible live in the UI.

## Out of scope

- Distributed execution, external queues, schedulers (Quartz/Hangfire) — revisit only if in-process ever proves insufficient.

## Result

Designed in [2026-08-14-job-engine-design.md](../superpowers/specs/2026-08-14-job-engine-design.md).

**Schema** — one `jobs` table, **one row per work unit rather than per run**, with a unique index on
`(Kind, TargetKey)`. Enqueueing a unit that already exists upserts it back to `Pending`. That is
what the spec means by a unit recording the version that last succeeded: it bounds the table by how
much work exists instead of by how often it has been replayed, and it makes step 3's sweep one
statement. The cost is that no per-run history is kept — a job shows its last outcome only. Nothing
in steps 3–7 asks for more; a `job_runs` table can be added later without disturbing this one.
`State` persists as the enum's **name**, so inserting a member mid-enum cannot reinterpret existing
rows.

**Queue** — Postgres is the queue; the bounded `Channel<Guid>` sits *downstream* of it as a dispatch
buffer. The step's original sketch had enqueue write the row and the channel together, which caps
the backlog at the channel's capacity — untenable once step 3 enqueues a job per catalog entry. A
pump claims a batch with `SELECT … FOR UPDATE SKIP LOCKED` in a single statement and feeds the
channel; `SKIP LOCKED` is what makes two claimers take disjoint sets rather than block. It claims
only what the channel can immediately take, so rows never sit marked `Running` in a buffer nobody is
working on. Enqueue signals the pump after the commit, never before.

**Recovery** — on startup, everything left `Running` or `Stale` returns to `Pending`. `Attempts` is
untouched: only one process runs, so a `Running` row found at startup was killed rather than tried,
and charging it an attempt would turn a restart into a permanent failure.

**Failure** — one attempt, then the job parks in `Failed` with its error and waits for the retry
button. Shutdown is not failure: a handler cancelled by the host token is released back to `Pending`.

**Idempotency** — the job's terminal write and the importer's transaction are separate commits, so a
crash between them is possible. It is safe either way: crash before the import commits and the retry
re-runs against blobs that dedupe on write; crash after it commits and `PackageImporter`'s existing
duplicate-archive check returns without doing work. No second mechanism was added for this.

**Import** — the page uploads and queues, and that is all; a browser refresh mid-import no longer
abandons anything, because there is nothing on the circuit to abandon. The handler owns the staged
archive from the point it is queued.

**Staleness** — the registry and `(ComponentId, ComponentVersion)` stamping are in place; the
invalidation sweep is step 3's. Unpack is not a stale-able unit — re-unpacking yields byte-identical
blobs — so there was nothing here to bump a version against and prove a sweep with. `Stale` is
already recovered on startup even though nothing produces it yet.

**UI** — the notifier says only that something changed and the page re-reads, so there is never a
second representation of a row that can disagree with the table. Bursts are **coalesced, not
throttled**: a leading-edge throttle drops the terminal notification and leaves a finished job
displayed as running, which is exactly what the first hand-run of the page did before `RefreshWindow`
was introduced. It reads on the way out of the interval instead.

Verified by `JobRecoveryTests`, which kills a runner mid-unpack of a 400-file archive, forces the row
back to `Running` the way a dead process would leave it, and asserts a fresh runner finishes with
exactly one package and no duplicated or lost files. Also run by hand against a real Postgres:
archives of 300, 1 200, 7 000 and 9 000 files were queued while the jobs page was open, and each was
picked up unprompted, ticked its progress live, and settled on `Succeeded` without a reload.

**Known limits:** a permanently failed unpack keeps its archive in `incoming/` so a retry has
something to read, and nothing cleans those up. There is no per-run history. The UI shows the most
recent 100 units, unpaged.
