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
| `docs/` | Spec, roadmap, a detail document per roadmap step, and [export guides](docs/guide/Readme.md) per provider. |
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

It is an **input cache**, not a store of record. It goes somewhere reachable so things can be
entered from anywhere; the homelab pulls from it over the API below and does the real keeping.

Everything it knows lives in one SQLite file, created on first run. Copy that file away and that is
your backup. There are no EF migrations behind it: one file, one person, so a schema change means
moving the old file aside rather than maintaining a migration chain.

`src/tracking.slnx` holds just these projects; `dotnet test src/tracking.slnx` runs the unit tests
without needing Docker or Postgres.

### Running it

The app refuses to start without a password hash, so make one first. It reads from stdin, which
keeps the password out of your shell history:

```bash
dotnet run --project src/Fip.Strive.Tracking.Web -- hash-password
```

Then, locally:

```bash
Access__PasswordHash='pbkdf2-sha256$...' dotnet run --project src/Fip.Strive.Tracking.Web
```

Or in a container — built from the repository root, because the project inherits its build files
from `src/`:

```bash
docker build -f src/Fip.Strive.Tracking.Web/Dockerfile -t strive-tracking .

docker run -d -p 8080:8080 -v strive-tracking-data:/data \
  -e "Access__PasswordHash=$(echo 'your password' | docker run --rm -i strive-tracking hash-password)" \
  -e "Access__ApiKey=$(openssl rand -hex 32)" \
  strive-tracking
```

The database lands in `/data`, which is where the volume goes. The image runs as a non-root user, so
a bind mount instead of a named volume has to be chowned on the host first. Readiness is at
`/health`, which stays public.

**Put it behind TLS.** The session cookie is issued `Secure` outside Development, so sign-in simply
will not work over plain HTTP — which is the intended failure.

### Getting in

One user, one password, no user table — the hash lives in configuration. Signing in sets a cookie
and everything except `/login.html`, `/auth/*`, `/health` and the API needs it. The login form is
rate limited to 10 attempts per 5 minutes per caller address; behind a reverse proxy that is one
bucket for everyone, which still bounds a brute force.

| Setting | Env var | Default | What |
|---|---|---|---|
| `Tracking:DataDirectory` | `Tracking__DataDirectory` | `data` | Directory holding the database file. Relative paths resolve against the content root, absolute paths are used as-is. |
| `Tracking:DatabaseFileName` | `Tracking__DatabaseFileName` | `tracking.db` | Name of the SQLite file inside that directory. |
| `Access:PasswordHash` | `Access__PasswordHash` | — | **Required.** PBKDF2 hash from `hash-password`. |
| `Access:ApiKey` | `Access__ApiKey` | — | Key for the pull API. Empty means the API is not mapped at all. At least 32 characters. |
| `Access:UserName` | `Access__UserName` | `admin` | Cosmetic — shown in the app bar. |
| `Access:SessionDays` | `Access__SessionDays` | `14` | How long a sign-in lasts. Sliding. |

### Pulling the data out

Read-only, JSON, authenticated with an `X-Api-Key` header rather than the cookie:

```bash
curl -H "X-Api-Key: $KEY" https://tracker.example/api/v1/trackers
curl -H "X-Api-Key: $KEY" "https://tracker.example/api/v1/events?since=2026-08-13T07:00:00%2B00:00"
```

`GET /api/v1/trackers` returns every tracker with its field definitions.
`GET /api/v1/events` returns events oldest-first with their values, taking `since`, `trackerId`,
`skip` and `take` (max 1000).

Two things to know before writing the puller:

- `since` filters on **`recordedUtc`**, not `occurredUtc`, and is **inclusive**. An event entered
  today for last Tuesday has to reach a puller that already synced past last Tuesday, and the
  inclusive boundary means the last event of a page arrives once more on the next pull. Delivery is
  at-least-once; dedupe on `id`. Pass the response's `nextSince` back as the next `since`.
- Deleting an event here is invisible to a puller — there are no tombstones. If that matters,
  re-pull from scratch periodically or treat the homelab as append-only.
