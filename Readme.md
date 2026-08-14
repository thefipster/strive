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
| `src/*.UnitTests` | xunit projects, one per application layer. No infrastructure — they run anywhere. |
| `src/Fip.Strive.IntegrationTests` | Boots the real host against a throwaway Postgres via Testcontainers. Needs Docker. |
| `docs/` | Spec, roadmap, a detail document per roadmap step, and [export guides](docs/guide/Readme.md) per provider. |
| `legacy/` | Two earlier generations, read-only reference. See [legacy/Readme.md](legacy/Readme.md). |
| `testdata/` | Seed corpus of real exports. Local only, never committed. |

## Working with it

Everything needs the .NET 10 SDK, pinned in `src/global.json`. There are two solutions, and they
have deliberately different prerequisites — the tracker is meant to be workable with nothing
installed but the SDK.

```bash
dotnet tool restore
```

Once, after cloning. That fetches CSharpier, which is what formats this repository; the version is
pinned in `.config/dotnet-tools.json` so everyone gets the same one, and CI rejects anything it
would reformat.

```bash
dotnet run --project src/Fip.Strive.AppHost
```

Starts the Aspire AppHost, which brings up Postgres (persistent volume, pgAdmin on :8081) and the
platform app together. **Needs Docker.**

| Task | Command | Needs Docker |
|---|---|---|
| Platform app + Postgres | `dotnet run --project src/Fip.Strive.AppHost` | yes |
| Tracker | `dotnet run --project src/Fip.Strive.Tracking.Web` | no |
| Everything, both solutions | `dotnet test src/strive.slnx` | yes — Testcontainers |
| Tracker only | `dotnet test src/tracking.slnx` | no |
| Unit tests only, no infrastructure | `dotnet test src/Fip.Strive.Application.UnitTests` | no |
| Check formatting | `dotnet csharpier check src/` | no |
| Reformat | `dotnet csharpier format src/` | no |

`dotnet test src/strive.slnx` fails rather than skips without a Docker daemon: the integration
suite starts a throwaway Postgres and there is no fallback. Run `tracking.slnx` or the unit test
project directly when Docker is not available.

To run the platform app on its own, point it at any Postgres:

```bash
ConnectionStrings__strive="Host=localhost;Database=strive;Username=postgres;Password=postgres" dotnet run --project src/Fip.Strive.Web
```

**The platform app has no authentication.** Every page, including the upload form that accepts
multi-gigabyte archives, is open to anyone who can reach the port. That is a deliberate choice for
a homelab tool sitting on a trusted LAN, and it is the opposite of the choice the tracker makes —
so keep it on the LAN and never expose it. The tracker is the one built to face the internet.

## Configuration

| Setting | Env var | Default | What |
|---|---|---|---|
| `ConnectionStrings:strive` | `ConnectionStrings__strive` | — | Postgres connection. Supplied by the AppHost in development; required otherwise. |
| `Storage:DataDirectory` | `Storage__DataDirectory` | `data` | Root for everything written to disk — `blobs/` and `incoming/` hang off it. Relative paths resolve against the content root, absolute paths are used as-is. |
| `Storage:MaxUploadBytes` | `Storage__MaxUploadBytes` | 8 GiB | Largest archive the upload page accepts. Bounds the *compressed* upload. |
| `Storage:MaxArchiveEntries` | `Storage__MaxArchiveEntries` | 500,000 | How many files one archive may contain. |
| `Storage:MaxEntryBytes` | `Storage__MaxEntryBytes` | 4 GiB | Largest single file an archive may unpack to. |
| `Storage:MaxTotalUncompressedBytes` | `Storage__MaxTotalUncompressedBytes` | 64 GiB | Largest total an archive may unpack to. |
| `Jobs:Enabled` | `Jobs__Enabled` | `true` | Whether the background job runner starts. Off only for tests that drive jobs themselves. |
| `Jobs:Parallelism` | `Jobs__Parallelism` | processors, max 8 | Concurrent job workers. |
| `Jobs:PollInterval` | `Jobs__PollInterval` | `00:00:05` | How long the pump waits for a signal before looking anyway. |
| `Jobs:ProgressInterval` | `Jobs__ProgressInterval` | `00:00:00.5` | Floor between persisted progress writes. |

The three expansion limits are what a compression bomb actually meets. `MaxUploadBytes` bounds what
arrives, which says nothing about what it becomes — a megabyte of zeros compresses to a few hundred
bytes. Defaults sit well above any real vendor takeout; raise them if a genuine export is refused.

Unpacking runs as a background job, not on the request that uploaded the archive. Closing the
browser tab during a run has no effect on it, and a run interrupted by a restart resumes from the
job table. Watch progress on `/jobs`, which is also where a failed job is retried.

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

### Making a password hash

Configuration holds a hash, never the password, so the app refuses to start until there is one.
`hash-password` is what turns one into the other. It reads from stdin, which keeps the password out
of your shell history and out of the process list:

```bash
dotnet run --project src/Fip.Strive.Tracking.Web -- hash-password
```

It prints a single line looking like `pbkdf2-sha256$210000$<salt>$<hash>`. That whole line is the
value, `$` signs included.

**Paste it inside single quotes.** The hash contains `$210000$`, and both bash and PowerShell expand
`$` inside double quotes, so `"pbkdf2-sha256$210000$abc"` silently becomes `pbkdf2-sha25610000`. The
app then rejects it as "missing or not a PBKDF2 hash" while the value looks perfectly fine on the
screen you copied it from.

This applies to pasted literals only. Piping the command's output straight into something, as the
`docker run` below does, is safe either way — a command substitution's result is not expanded again.

It also takes the password as an argument, which is handy in a pipe and a bad idea interactively:

```bash
docker run --rm -i strive-tracking hash-password < secret.txt
```

### Running it

Locally, with the hash on the command line:

```bash
Access__PasswordHash='pbkdf2-sha256$...' dotnet run --project src/Fip.Strive.Tracking.Web
```

From an IDE, use user secrets instead — the hash lives outside the repository entirely and you stop
having to supply it on every run:

```bash
dotnet user-secrets set 'Access:PasswordHash' 'pbkdf2-sha256$...' --project src/Fip.Strive.Tracking.Web
```

Then just run the `http` or `https` profile. Both set `ASPNETCORE_ENVIRONMENT=Development`, which is
what makes the host read user secrets at all, and is also why plain `http://localhost:5230` works
here when it would not in production — the session cookie is only marked `Secure` outside
Development.

Two things that will otherwise cost you an afternoon:

- **The first run after enabling user secrets has to be a real build.** The `UserSecretsId` reaches
  the assembly as a compile-time attribute, so running against a stale binary fails with the same
  "missing or not a PBKDF2 hash" message as having no hash at all.
- **Do not put the hash in `launchSettings.json` or `appsettings.Development.json`.** Both are
  committed. This matters in Rider specifically: its run configuration for this project *is* the
  launch settings profile, so anything typed into that configuration's environment-variables box is
  written straight into `launchSettings.json` and committed with it.

Or in a container — built from the repository root, because the project inherits its build files
from `src/`:

```bash
docker build -f src/Fip.Strive.Tracking.Web/Dockerfile -t strive-tracking .

docker run -d -p 8080:8080 -v strive-tracking-data:/data \
  -e "Access__PasswordHash=$(echo 'your password' | docker run --rm -i strive-tracking hash-password)" \
  -e "Access__ApiKey=$(openssl rand -hex 32)" \
  -e "Proxy__Enabled=true" \
  strive-tracking
```

The database lands in `/data`, which is where the volume goes. The image runs as a non-root user, so
a bind mount instead of a named volume has to be chowned on the host first.

Readiness is at `/health`. It queries the database rather than reporting on the process, so it goes
red when the SQLite file is missing, locked or short of the schema the app expects — which is worth
knowing, given a schema change here means moving the old file aside rather than migrating it. When
`Access:ApiKey` is set the endpoint wants the same `X-Api-Key` header the pull API does, so the
probe has to carry it:

```bash
curl -fsS -H "X-Api-Key: $KEY" https://tracker.example/health
```

With no API key configured there is nothing to authenticate with, and `/health` stays public — a
probe that could only ever get a 401 would read as a permanently unhealthy container. The app says
which of the two it is in its startup log.

**Put it behind TLS.** The session cookie is issued `Secure` outside Development, so sign-in simply
will not work over plain HTTP — which is the intended failure. It is a quiet one, though: the
browser discards the cookie and bounces back to the login page with nothing logged and no error
shown. The app therefore says so at startup when it sees no HTTPS endpoint and no proxy configured.

**And tell it about the proxy.** Terminating TLS in front of the container means the app itself only
ever sees plain HTTP from the proxy's address, and two things quietly stop working: HSTS is never
sent — the header is only added to requests that already look secure — and the login rate limiter
puts every caller on the internet in a single bucket, so one attacker exhausting the allowance locks
the real user out. `Proxy__Enabled=true` makes the app read `X-Forwarded-Proto` and
`X-Forwarded-For`, which fixes both and puts real client addresses in the log.

It is off by default on purpose. Honouring `X-Forwarded-For` from a caller who is *not* a trusted
proxy is worse than ignoring it: anyone able to reach the app directly could vary the header per
request and step around the rate limit entirely. Only turn it on when nothing but the proxy can
reach the app, and name `Proxy__KnownProxies__0` or `Proxy__KnownNetworks__0` when the proxy has a
stable address.

### Getting in

One user, one password, no user table — the hash lives in configuration. Signing in sets a cookie
and everything except `/login.html`, `/auth/*`, `/health` and the API needs it. Those last two are
not open, they are keyed rather than cookied — both want `X-Api-Key`. The login form is
rate limited to 10 attempts per 5 minutes per caller address — which is a per-caller limit only when
`Proxy:Enabled` is on. Without it every request carries the proxy's address, so the ten attempts are
shared by everyone and one attacker can lock you out of your own login.

| Setting | Env var | Default | What |
|---|---|---|---|
| `Tracking:DataDirectory` | `Tracking__DataDirectory` | `data` | Directory holding the database file. Relative paths resolve against the content root, absolute paths are used as-is. |
| `Tracking:DatabaseFileName` | `Tracking__DatabaseFileName` | `tracking.db` | Name of the SQLite file inside that directory. |
| `Access:PasswordHash` | `Access__PasswordHash` | — | **Required.** PBKDF2 hash from `hash-password`. |
| `Access:ApiKey` | `Access__ApiKey` | — | Key for the pull API. Empty means the API is not mapped at all. At least 32 characters. |
| `Access:UserName` | `Access__UserName` | `admin` | Cosmetic — shown in the app bar. |
| `Access:SessionDays` | `Access__SessionDays` | `14` | How long a sign-in lasts. Sliding. |
| `Proxy:Enabled` | `Proxy__Enabled` | `false` | Honour `X-Forwarded-Proto` and `X-Forwarded-For`. Turn on when TLS is terminated in front of the app, and only when nothing else can reach it. |
| `Proxy:KnownProxies` | `Proxy__KnownProxies__0` | — | Proxy addresses to trust. Empty trusts the immediate upstream, which is usually what a container network forces. |
| `Proxy:KnownNetworks` | `Proxy__KnownNetworks__0` | — | Same, in CIDR form. |

### Pulling the data out

Read-only, JSON, authenticated with an `X-Api-Key` header rather than the cookie:

```bash
curl -H "X-Api-Key: $KEY" https://tracker.example/api/v1/trackers
curl -H "X-Api-Key: $KEY" "https://tracker.example/api/v1/events?since=2026-08-13T07:00:00%2B00:00"
```

`GET /api/v1/trackers` returns every tracker with its field definitions.
`GET /api/v1/events` returns events oldest-first with their values, taking `since`, `trackerId`,
`skip` and `take` (max 1000).

`since` filters on `RecordedUtc`, not `OccurredUtc` — an event backdated to last week is still news
to a puller that synced yesterday. It is an **inclusive** lower bound, and the `nextSince` in each
response is the last row's `RecordedUtc`, so following it re-delivers that final event on the next
pull. Deduplicate on event id; the ids are stable.

Two things to know before writing the puller:

- `since` filters on **`recordedUtc`**, not `occurredUtc`, and is **inclusive**. An event entered
  today for last Tuesday has to reach a puller that already synced past last Tuesday, and the
  inclusive boundary means the last event of a page arrives once more on the next pull. Delivery is
  at-least-once; dedupe on `id`. Pass the response's `nextSince` back as the next `since`.
- Deleting an event here is invisible to a puller — there are no tombstones. If that matters,
  re-pull from scratch periodically or treat the homelab as append-only.
