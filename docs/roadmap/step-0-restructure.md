# Step 0 — Restructure repository, new solution, UI shell

## Goal

Park the previous attempt as reference code, start a clean solution, and get an empty but running Blazor Server app with the basic layout in place.

## Background

The repository already contains two generations: the original ActivityAggregator (currently `src/legacy/`) and the distributed Harvester pipeline (current `src/`, `test/`). Both become read-only quarries. The distributed runtime (RabbitMQ/Redis/worker CLIs/Aspire) is deliberately not carried forward; the ingestion domain knowledge (classifiers, probe, extractor format logic) will be ported in later steps.

## Tasks

- [ ] Create `legacy/` with room for both generations:
  - [ ] Move `src/legacy/TheFipster.ActivityAggregator.*` → `legacy/aggregator/`
  - [ ] Move the current solution (`src/`, `test/`, `strive.sln`) → `legacy/harvester/`
- [ ] Leave build output behind: no `bin/`/`obj/` trees make the move (the tree currently carries stale net9.0 artifacts).
- [ ] Keep the seed test corpus at top level: `test/data/` (13 real Polar Flow exports, TheFipsterApp samples) stays out of `legacy/`, e.g. as `testdata/`.
- [ ] Drop known dead weight instead of moving it:
  - [ ] `test/queue/Fip.Strive.Queue.Application.UnitTests` (references deleted projects, not in the solution)
  - [ ] Empty `src/queue/`, `src/indexing/`, stray `src/Fip.Strive.AppHost/` husks
  - [ ] Unifier/Portal stub projects (empty shells, superseded by this roadmap)
- [ ] Create the new solution `strive.sln` at the root:
  - [ ] Keep central package management: `Directory.Packages.props`, `Directory.Build.props` (net10.0, nullable, implicit usings)
  - [ ] New Blazor Server project with MudBlazor: top appbar with title, nav menu drawer
  - [ ] Health endpoint, Serilog logging (port the minimal parts of `Core.Web` that earn their keep)
- [ ] Fix CI: `.github/workflows/strive.yml` still pins .NET 9 and cannot build the solution — update to .NET 10, build the new solution.

## Done criterion

- The new solution builds and tests pass locally **and** in CI.
- The app starts and shows the MudBlazor shell (appbar + nav menu).
- Legacy code is fully out of the build but present under `legacy/` for reference.

## Out of scope

- Any pipeline functionality, database, or docker packaging.
