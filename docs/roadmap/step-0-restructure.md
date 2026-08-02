# Step 0 — Restructure repository, new solution, UI shell

## Goal

Park the previous attempt as reference code, start a clean solution, and get an empty but running Blazor Server app with the basic layout in place.

## Background

The repository already contains two generations: the original ActivityAggregator (currently `src/legacy/`) and the distributed Harvester pipeline (current `src/`, `test/`). Both become read-only quarries. The distributed runtime (RabbitMQ/Redis/worker CLIs/Aspire) is deliberately not carried forward; the ingestion domain knowledge (classifiers, probe, extractor format logic) will be ported in later steps.

## Tasks

- [x] Create `legacy/` with room for both generations:
  - [x] Move `src/legacy/TheFipster.ActivityAggregator.*` → `legacy/aggregator/`
  - [x] Move the current solution (`src/`, `test/`, `strive.sln`) → `legacy/harvester/`
- [x] Leave build output behind: no `bin/`/`obj/` trees make the move (the tree currently carries stale net9.0 artifacts).
- [x] Keep the seed test corpus at top level: `test/data/` (13 real Polar Flow exports, TheFipsterApp samples) stays out of `legacy/`, e.g. as `testdata/`.
- [x] Drop known dead weight instead of moving it:
  - [x] `test/queue/Fip.Strive.Queue.Application.UnitTests` (references deleted projects, not in the solution)
  - [x] Empty `src/queue/`, `src/indexing/`, stray `src/Fip.Strive.AppHost/` husks
  - [x] Unifier/Portal stub projects (empty shells, superseded by this roadmap)
- [x] Create the new solution `strive.sln` at the root:
  - [x] Keep central package management: `Directory.Packages.props`, `Directory.Build.props` (net10.0, nullable, implicit usings)
  - [x] New Blazor Server project with MudBlazor: top appbar with title, nav menu drawer
  - [x] Health endpoint, Serilog logging (port the minimal parts of `Core.Web` that earn their keep)
- [x] Fix CI: `.github/workflows/strive.yml` still pins .NET 9 and cannot build the solution — update to .NET 10, build the new solution.

## Done criterion

- The new solution builds and tests pass locally **and** in CI.
- The app starts and shows the MudBlazor shell (appbar + nav menu).
- Legacy code is fully out of the build but present under `legacy/` for reference.

## Out of scope

- Any pipeline functionality, database, or docker packaging.

## Result

New solution: `src/Fip.Strive.Web` (Blazor Server) + `src/Fip.Strive.Application` (application
layer seam), with an xunit project per source project under `test/`. `Fip.Strive.Web.UnitTests`
boots the host through `WebApplicationFactory` and asserts `/health` reports `Healthy` and that
the home page renders the MudBlazor app bar and drawer — so the done criterion is checked
mechanically on every CI run.

Both legacy generations carry their own `Directory.Build.props` / `Directory.Packages.props`,
so they stay restorable in isolation while the root package manifest was pruned to just what
the new solution uses.

Deferred deliberately: MudBlazor stays on 8.x (9.x is out) — a major UI framework bump does not
belong in a restructure step, where it would blur the signal of whether the move itself worked.
