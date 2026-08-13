# Strive

Aggregator for different activity sources from Polar, Garmin, Withings and some more...

Strive is being rebuilt incrementally against the
[wearable data platform spec](docs/wearable-data-platform-spec.md), one step at a time — see the
[roadmap](docs/roadmap.md). Everything runs in a single in-process Blazor Server app.

## Helpful links

[Filtered Issues](https://github.com/thefipster/strive/issues?q=is%3Aissue%20state%3Aopen%20-label%3Atask)

## Repository layout

| Path | What |
|---|---|
| `src/Fip.Strive.Web` | Blazor Server app — the whole runtime. MudBlazor shell, health endpoint, Serilog. |
| `src/Fip.Strive.Application` | Application layer: catalog, blob store, import. Pipeline features land here as roadmap steps complete. |
| `src/Fip.Strive.AppHost` | Aspire orchestration for development only — brings up Postgres alongside the app. |
| `src/Fip.Strive.Tracking.Web` | The tracker — a second, standalone Blazor Server app. Shares nothing with the platform above but the repo. |
| `src/Fip.Strive.Tracking.Application` | Its application layer: trackers, custom fields, events. Stores everything in one SQLite file. |
| `test/` | xunit projects: pure unit tests, plus integration tests against real Postgres via Testcontainers. |
| `docs/` | Spec, roadmap, and a detail document per roadmap step. |
| `legacy/` | Two earlier generations, read-only reference. See [legacy/Readme.md](legacy/Readme.md). |
| `testdata/` | Seed corpus of real exports. Local only, never committed. |

## Working with it

```powershell
./make.ps1 run
```

That starts the Aspire AppHost, which brings up Postgres (with a persistent data volume and
pgAdmin on :8081) and the web app together. Requires the .NET 10 SDK (pinned in `global.json`) and
a running Docker — also needed by `./make.ps1 test`, whose integration tests spin up a throwaway
Postgres.

To run the web app on its own, point it at any Postgres:

```bash
ConnectionStrings__strive="Host=localhost;Database=strive;Username=postgres;Password=postgres" dotnet run --project src/Fip.Strive.Web
```

## Configuration

| Setting | Env var | Default | What |
|---|---|---|---|
| `ConnectionStrings:strive` | `ConnectionStrings__strive` | — | Postgres connection. Supplied by the AppHost in development; required otherwise. |
| `Storage:DataDirectory` | `Storage__DataDirectory` | `data` | Root for everything written to disk — `blobs/` and `incoming/` hang off it. Relative paths resolve against the content root, absolute paths are used as-is. |
| `Storage:MaxUploadBytes` | `Storage__MaxUploadBytes` | 8 GiB | Largest archive the upload page accepts. |

The schema is migrated on startup.

## The tracker

A second app in the same repo, deliberately unconnected to the platform above: no Postgres, no
Docker, no Aspire. Create a tracker for anything worth counting, give it whatever custom fields you
want alongside the timestamp — decimal numbers with an optional unit, or free text — and log events
against it. Numbers are totalled, averaged and bracketed per field on the tracker's page.

```bash
dotnet run --project src/Fip.Strive.Tracking.Web
```

Or in a container — built from the repository root, because the project inherits its build files
from `src/`:

```bash
docker build -f src/Fip.Strive.Tracking.Web/Dockerfile -t strive-tracking .
docker run -d -p 8080:8080 -v strive-tracking-data:/data strive-tracking
```

Everything it knows lives in one SQLite file, created on first run — in the image at `/data`, which
is where the volume goes. Copy that file away and that is your backup. The image runs as a non-root
user, so a bind mount instead of a named volume has to be chowned on the host first. Readiness is
at `/health`.

`src/tracking.slnx` holds just these projects; `dotnet test src/tracking.slnx` runs the unit tests
without needing Docker or Postgres.

| Setting | Env var | Default | What |
|---|---|---|---|
| `Tracking:DataDirectory` | `Tracking__DataDirectory` | `data` | Directory holding the database file. Relative paths resolve against the content root, absolute paths are used as-is. |
| `Tracking:DatabaseFileName` | `Tracking__DatabaseFileName` | `tracking.db` | Name of the SQLite file inside that directory. |

There are no EF migrations behind it: one file, one person, so a schema change means moving the old
file aside rather than maintaining a migration chain.
