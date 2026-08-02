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
