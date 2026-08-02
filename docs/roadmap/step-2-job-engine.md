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

- [ ] Job table schema + repository
- [ ] `BackgroundService` + bounded channel executor with configurable parallelism
- [ ] Startup recovery (re-enqueue `running`/`stale`)
- [ ] Staleness mechanic: component registry with versions; version change ⇒ mark units stale
- [ ] Live jobs page (server push updates)
- [ ] Retrofit unpacking as a job kind

## Done criterion

Start a large unpack run, **kill the app mid-run, restart** — the run resumes and completes with no duplicate or lost work. Closing the browser tab during a run has no effect on it. Job progress and errors are visible live in the UI.

## Out of scope

- Distributed execution, external queues, schedulers (Quartz/Hangfire) — revisit only if in-process ever proves insufficient.
