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
| `src/Fip.Strive.Application` | Application layer: pipeline features land here as roadmap steps complete. |
| `test/` | xunit test projects, one per source project. |
| `docs/` | Spec, roadmap, and a detail document per roadmap step. |
| `legacy/` | Two earlier generations, read-only reference. See [legacy/Readme.md](legacy/Readme.md). |
| `testdata/` | Seed corpus of real exports. Local only, never committed. |

## Working with it

```powershell
./make.ps1 build
./make.ps1 run
./make.ps1 test
```

Or plain `dotnet build strive.sln` / `dotnet test strive.sln`. Requires the .NET 10 SDK
(pinned in `global.json`).
