# Repository Review — August 2026

A full read-through of the repository as of `2dbe5ce`, covering both applications (`strive` and
`tracking`), their build files, CI, container and documentation. Findings are grouped by area and
carry a severity and a checkbox, so this document doubles as the work list.

**Scope note.** This was a static review — no .NET SDK was available in the review environment, so
nothing here was verified by building or running the test suite. Every finding was derived from
reading the source. Items marked *(verify)* need a build or a run to confirm before acting on them.

**Severity.** `high` = wrong behaviour or a real exposure; `medium` = will bite as data or usage
grows, or a real operational hazard; `low` = hygiene, consistency, polish.

## What is in good shape

Worth saying first, because it shapes how the rest should be read. The blob store's
hash-then-move-then-dedup path handles the concurrent-write race properly and cleans up its temp
file on every exit. The tracking app's crypto is correct: PBKDF2-HMAC-SHA256 at 210k iterations
with the cost encoded into the hash, `FixedTimeEquals` on verify, and a parser that returns `false`
rather than throwing on a malformed configuration value. The decision to convert every
`DateTimeOffset` to fixed-width UTC text for SQLite, and to normalise to UTC before writing, is
both correct and correctly explained. The import path never extracts to an archive-supplied path,
so zip-slip is structurally impossible. Comments throughout explain *why* rather than *what*.

The findings below are mostly about the seams: configuration, CI, operational edges, and the parts
of the second app that have not yet met a large dataset.

---

## A. Configuration & deployment

### A1 — `appsettings.json` ships a hardcoded Windows path *(high)*

`src/Fip.Strive.Web/appsettings.json:4` sets `Storage:DataDirectory` to `E:\strive\data`.

This is one developer's drive letter committed as the default for every environment. On Linux or in
a container it resolves to a *relative* directory literally named `E:\strive\data` under the content
root (backslashes are not separators there), so the app silently creates a junk folder instead of
failing. It also contradicts the Readme, which documents the default as `data`, and contradicts
`StorageOptions.DataDirectory`, whose own default is already `"data"`.

Remove the key from `appsettings.json` entirely and let the options default stand. If a local
machine needs `E:\strive\data`, that belongs in `appsettings.Development.json`, not in the shipped
defaults.

- [x] Remove `Storage:DataDirectory` from `src/Fip.Strive.Web/appsettings.json`

**Done.** The `Storage` section moved to `appsettings.Development.json`; `MaxUploadBytes` was
dropped along with it, since the value it carried was identical to the `StorageOptions` default.
Both apps' `appsettings.json` now hold only application defaults — no paths, no secrets — and
`appsettings.Development.json` is excluded from the Docker build context, so nothing
environment-specific can reach an image. Containers are configured by environment variable.

### A2 — `/health` reports healthy with no checks registered *(medium)*

Both apps call `services.AddHealthChecks()` and register nothing
(`Fip.Strive.Web/Setup/HealthCheckRegistration.cs:9`, same in the tracking app). `Predicate = _ => true`
then filters an empty set, so the endpoint returns `Healthy` unconditionally — including when
Postgres is unreachable or the SQLite file is unwritable.

This is not merely cosmetic. `AppHost.cs` wires `.WithHttpHealthCheck("/health")`, and the Dockerfile
points readiness at the same endpoint. Both are currently answering "is the process listening",
which the TCP probe already told us.

Add a real check per app: `AddDbContextCheck<StriveContext>()` for strive, and something equivalent
(a trivial `SELECT 1` or a write probe on the data directory) for tracking. Keep the endpoint cheap
— it is polled.

- [x] Register a database health check in `Fip.Strive.Web`
- [x] Register a database/data-directory health check in `Fip.Strive.Tracking.Web`

**Done.** Both apps got a check that queries rather than connects: `AnyAsync` against
`catalog_entries` on Postgres and against `trackers` on SQLite. `EXISTS … LIMIT 1` stops at the
first row, so neither grows more expensive as the data does, and both prove connection *and* schema
instead of just a reachable socket. Descriptions are deliberately vague — the host and the
connection error go to the log, not over the wire — and each check carries a 5s timeout so an
unreachable Postgres cannot park the probe on the connection timeout.

The tracker's endpoint now also sits behind the `X-Api-Key` filter, and moved from `MapHealthChecks`
to a route handler to get there: endpoint filters are only built for route handler delegates, so
`AddEndpointFilter` on a `RequestDelegate` endpoint compiles and never runs. With no API key
configured the endpoint stays public — there would be nothing to authenticate with, and a probe that
can only ever get a 401 reads as a permanently unhealthy container. Which of the two applies is in
the startup log.

Side effect worth noting against D1: the tracking check reads a table, so a database file left
behind by an older model now fails readiness at startup instead of surfacing on whichever page
touches the missing column first. That is a detector, not the fix D1 asks for — the message still
says "did not answer" rather than "your schema is stale".

### A3 — No Dockerfile for the main app *(medium)*

The tracker has a well-built, well-commented multi-stage Dockerfile that runs as non-root. The main
`Fip.Strive.Web` app — the one the roadmap describes as shipping as "a single self-contained
container" — has none. Today it can only be run from source or from the Aspire AppHost, which is
explicitly development-only.

Not urgent while the platform is mid-rebuild, but it is a gap between the documented deployment
story and what the repo can actually produce. The tracker's Dockerfile is a good template.

- [ ] Add `src/Fip.Strive.Web/Dockerfile`, modelled on the tracking one
- [ ] Build it in `strive.yml` so it cannot rot

### A4 — `NuGet.config` does not clear inherited sources *(low)*

Both `NuGet.config` files add `nuget.org` without a preceding `<clear />`. Machine- and user-level
sources therefore still apply, so a build on a machine with an extra feed configured can resolve
packages from somewhere the repository never named. Standard supply-chain hygiene is to clear
first, then add exactly the sources you intend.

- [ ] Add `<clear />` to both `NuGet.config` files

### A5 — No lock file, so restores are not reproducible *(low)*

Central package management pins versions, which is good, but transitive dependencies float. Setting
`RestorePackagesWithLockFile` in `Directory.Build.props` and committing `packages.lock.json` makes
a restore reproducible and makes transitive changes visible in review — which matters given the
repo already carries a note about dodging a vulnerable transitive `SQLitePCLRaw`.

- [ ] Enable `RestorePackagesWithLockFile` and commit the lock files
- [ ] Add `--locked-mode` to the CI restore steps

---

## B. CI & repository automation

### B1 — `strive.yml` path filters point at paths that do not exist *(medium)*

`.github/workflows/strive.yml` filters on `test/**`, `strive.slnx`, `Directory.Build.props`,
`Directory.Packages.props`, `NuGet.config` and `global.json` — all at repository root. Every one of
those actually lives under `src/`, and there is no `test/` directory at all (test projects sit in
`src/` beside everything else).

The workflow still fires today only because the `src/**` entry happens to cover all of them. The
filters are dead weight that reads as intentional coverage, and the moment `src/**` is narrowed they
become a silent gap. `tracking.yml` gets the same paths right, so this is a straightforward
correction.

- [ ] Fix the path filters in `strive.yml` to `src/...`, drop `test/**`

### B2 — `strive.yml` never runs on branch pushes *(medium)*

`tracking.yml` deliberately runs on every branch, with a comment explaining why. `strive.yml` runs
only on pushes to `main` and on PRs targeting `main`. Work on a feature branch therefore gets no
build signal at all until a pull request exists — which is exactly backwards from what the tracker's
comment argues for.

Either mirror the tracker's trigger, or write down why the two apps are treated differently.

- [ ] Make `strive.yml` build on branch pushes, or document the asymmetry

### B3 — Dependabot almost certainly updates nothing *(medium, verify)*

`.github/dependabot.yml` points the NuGet ecosystem at `directory: "/"`. There is no project,
solution or manifest at the repository root — everything is under `src/`, and the solutions are
`.slnx`, whose Dependabot support is newer and thinner than `.sln`.

Two things follow. First, point the ecosystem at `/src` (or use `directories:` with an explicit
list) and confirm from the Dependabot run log that it actually discovers the projects. Second, there
is no `github-actions` ecosystem entry at all, so `actions/checkout@v4` and `actions/setup-dotnet@v4`
will never be bumped.

- [ ] Repoint the NuGet ecosystem at `/src` and verify against a real Dependabot run
- [ ] Add a `github-actions` ecosystem entry

### B4 — No security or quality gates in CI *(medium)*

The pipelines restore, build and test. Nothing checks for vulnerable packages, and there is no code
scanning. For an application the Readme explicitly describes as "meant to sit on the open internet",
those are cheap additions:

- `dotnet list package --vulnerable --include-transitive` as a failing step
- CodeQL on the default branch and on PRs
- an explicit `permissions: contents: read` block on both workflows (the default token grant is
  wider than either job needs)
- SHA-pinned action versions

- [ ] Add a vulnerable-package check to both workflows
- [ ] Add a CodeQL workflow
- [ ] Add least-privilege `permissions` blocks and pin actions by SHA

### B5 — No formatting or analyzer enforcement *(low)*

The code is formatted with unusual consistency — clearly a tool is being run — but there is no
`.editorconfig` anywhere in the repository and no CI check, so the convention lives only in the
author's local setup. `Directory.Build.props` also enables no analyzers, yet
`Features/Catalog/Models/PackageFile.cs` carries a `[SuppressMessage("Usage", "CA2227")]` for a rule
that is not switched on.

- [ ] Add an `.editorconfig` capturing the current style
- [ ] Add a `dotnet format --verify-no-changes` step to CI
- [ ] Enable `EnableNETAnalyzers` / set an `AnalysisLevel`, then revisit the stale suppression

---

## C. Import pipeline (`Fip.Strive.Application`)

### C1 — No bound on archive expansion *(high)*

`Storage:MaxUploadBytes` (8 GiB by default) bounds the *compressed* archive. `PackageImporter.UnpackAsync`
then streams every entry to disk with no limit on entry count, on any single entry's uncompressed
size, or on the total. A 1 MB archive can expand to hundreds of gigabytes, and the import page that
accepts it is unauthenticated (see C4).

The zip-slip case is handled well — nothing is written to an archive-supplied path — but expansion
is not the same problem. `ZipArchiveEntry.Length` gives the declared uncompressed size before
reading, which is enough for a cheap pre-flight check; the belt-and-braces version also caps bytes
actually written and aborts mid-stream when the cap is passed, since the declared length is
attacker-supplied.

Suggested shape: configurable `MaxEntryCount`, `MaxEntryBytes` and `MaxTotalUncompressedBytes`,
defaulted generously enough that real takeouts pass, and a clear error when they do not.

- [ ] Add expansion limits to `PackageImporter` with configurable ceilings
- [ ] Cover them with a test using a small crafted high-ratio archive

### C2 — Whole manifest is built and saved in one go *(medium)*

`UnpackAsync` accumulates a `List<ManifestLine>` for every file in the archive, and `RecordAsync`
adds every `PackageFile` and every new `CatalogEntry` to the change tracker before a single
`SaveChangesAsync` (`PackageImporter.cs:144`). A vendor takeout with a few hundred thousand files
means that many tracked entities in memory, and one very large transaction.

The single transaction is a deliberate, well-argued choice — a package must never exist
half-catalogued — and should be kept. Batching the *inserts* inside that transaction, or moving to a
bulk-copy path for the manifest, gets the memory back without giving up the atomicity. Worth doing
before step 2 puts this on the job engine.

- [ ] Batch manifest inserts inside the existing transaction
- [ ] Measure against the largest real takeout in the corpus first

### C3 — Concurrent import of the same archive surfaces a raw EF exception *(low)*

`ImportAsync` checks for an existing package by `ArchiveHash` (`PackageImporter.cs:28`) and then
inserts. The unique index makes the outcome *safe* — two packages for the same bytes are impossible,
exactly as the configuration comment claims — but the losing writer gets a `DbUpdateException`
rather than the `DuplicateArchive` result it should get. The user sees a generic failure snackbar
for something that is not a failure.

Only reachable from two circuits at once (the page's `_busy` flag is per-circuit), so it is unlikely
in a single-user homelab. Cheap to fix: catch the unique-violation on save, re-read, and return
`DuplicateArchive`.

- [ ] Translate the unique-violation into `ImportOutcome.DuplicateArchive`

### C4 — The platform app has no authentication at all *(medium — decision needed)*

`Fip.Strive.Web` registers no authentication or authorization. Every page, including an upload form
that accepts 8 GiB archives and writes them to disk, is open to anyone who can reach the port.

The tracker was given a login precisely because it "is meant to sit on the open internet"; the
platform app appears to assume it never leaves the homelab LAN. That may well be the right call —
but it is currently an implicit assumption rather than a documented one, and the two apps in one
repository making opposite choices is exactly the kind of thing that gets forgotten at deployment
time.

Either state the assumption in the Readme ("LAN only, never expose"), or reuse the tracker's cookie
setup. This is a decision to make, not a defect to fix.

- [ ] Decide: document as LAN-only, or add authentication
- [ ] Record the decision wherever it lands

### C5 — Unreferenced blobs are never reclaimed *(low)*

Blobs are written before the catalog transaction, by design — a failed or cancelled import leaves
"simply unreferenced bytes that a retry deduplicates against", as the comment puts it. That is
sound, but nothing ever collects them. Cancelling a large import repeatedly accumulates dead bytes
in `blobs/` with no way to find or remove them. `package_files` deletion cascades likewise leave
`catalog_entries` and their blobs behind, deliberately.

No action needed today — nothing deletes packages yet. Worth a placeholder so it is a conscious
deferral rather than an oversight.

- [ ] Note blob GC as a prerequisite for any future package deletion

### C6 — Catalog search is case-sensitive and unindexed *(low)*

`CatalogReader.GetEntriesAsync` matches `entry.Hash.StartsWith(term)` (`CatalogReader.cs:53`).
Hashes are stored lowercase (`Convert.ToHexStringLower`), so pasting a hash copied from a tool that
emits uppercase finds nothing, with no hint as to why. Lowercase the term before querying.

The second half of the search, `Occurrences.Any(o => o.PathInArchive.Contains(term))`, becomes a
`LIKE '%…%'` inside an `EXISTS` over `package_files` — no index can serve it, and `package_files`
is the table that grows fastest. Fine at today's size; consider a trigram index or full-text search
when it stops being fine. Note also that `%` and `_` typed into the box act as wildcards, which is
harmless but surprising.

- [ ] Lowercase the search term before the hash comparison
- [ ] Revisit path search performance when `package_files` gets large

---

## D. Tracking app (`Fip.Strive.Tracking.*`)

### D1 — `EnsureCreated` silently ignores model changes *(medium)*

`TrackingSchema.EnsureCreatedAsync` creates tables on first run and does nothing thereafter. Skipping
migrations is a defensible call for a single-file, single-user database, and it is argued clearly in
the comment. The hazard is what happens on the day the model changes: `EnsureCreated` sees a file
that exists, does nothing, and the app starts happily — then fails at the first query with
`SQLite Error 1: no such column`. The documented remedy (move the old file aside) only helps someone
who already knows that is the cause.

Keep the no-migrations decision; make the failure legible. A `user_version` pragma stamped at
creation and checked at startup turns a confusing runtime error into a startup message that names
the problem and the fix.

- [ ] Stamp and verify a schema version at startup, failing fast with an actionable message

### D2 — Number statistics load every value into memory *(medium)*

`EventReader.GetNumberStatsAsync` fetches every number value of a tracker
(`EventReader.cs:75`) and aggregates in memory. The reason is correct and well documented — SQLite
stores a decimal as text, so `SUM` and `MIN` in SQL would compare strings — but the consequence is
that opening a tracker's page costs a full scan of its values, and this runs again after every
single recorded event (`RefreshAsync` → `RefreshStatsAsync`).

A tracker used daily for years is fine. One fed by an importer is not. Two independent fixes: store
numbers as `REAL` alongside the exact decimal (aggregate on the former, display the latter), or
cache the statistics and invalidate on write rather than recomputing on every render.

- [ ] Avoid recomputing statistics on every event record
- [ ] Consider a sortable numeric column if trackers ever grow large

### D3 — API key comparison leaks key length *(low)*

`AccessGuard.IsApiKey` calls `FixedTimeEquals` on the UTF-8 bytes of the candidate and the
configured key (`AccessGuard.cs:29`). `FixedTimeEquals` returns `false` immediately when the lengths
differ, so the comparison is constant-time *within* a length but distinguishes lengths in time. The
content is protected; the length is not.

The reasoning in the comment is right and the fix is small: hash both sides (`SHA256.HashData`) and
compare the digests, which are always 32 bytes. Marginal — a 32-character generated secret is not
falling to a length oracle — but it is three lines and it makes the intent exact.

- [ ] Compare fixed-length digests instead of raw bytes

### D4 — No upper bound on the PBKDF2 iteration count *(low)*

`Pbkdf2Password.Verify` accepts any positive iteration count from the encoded hash. The value comes
from configuration, not from an attacker, so this is a foot-gun rather than an exposure: a typo
adding a digit turns every login attempt into a multi-second CPU burn, and the login endpoint is
reachable pre-authentication. Clamp to a sane ceiling and reject above it.

- [ ] Clamp accepted iteration counts in `Verify`

### D5 — Name-uniqueness checks race against the unique index *(low)*

`TrackerWriter.GuardTrackerNameIsFreeAsync` and `GuardFieldNameIsFreeAsync` query, then insert.
Concurrently, both can pass and the second insert hits the unique index, producing a
`DbUpdateException` instead of the friendly `TrackingException` the pages know how to display. One
user, so it needs two tabs and unfortunate timing — but the pages catch only `TrackingException`, so
the result is an unhandled error rather than a snackbar.

Also worth noting: the case-insensitive check uses `ToLower()`, which SQLite applies to ASCII only.
"STRASSE" and "straße" will not be seen as colliding. Almost certainly irrelevant here; recorded so
it is a known limit rather than a surprise.

- [ ] Translate unique-index violations into `TrackingException`

### D6 — Random GUIDs as primary keys *(low)*

The tracking app uses `Guid.NewGuid()` for every key (`TrackerWriter.cs:20,98`,
`EventRecorder.cs:33,88`) while the platform app uses `Guid.CreateVersion7()`
(`PackageImporter.cs:130`). Version 7 GUIDs are time-ordered, so inserts append to the index instead
of scattering across it — which matters most for `tracker_events`, the table that grows without
bound.

The inconsistency between two apps in one repository is itself worth resolving, whichever way it
goes. Changing this does not require touching existing rows: both are GUIDs.

- [ ] Switch tracking to `Guid.CreateVersion7()` for consistency and index locality

### D7 — Login redirect discards the return URL *(low)*

`AccessEndpoints` always redirects to `/` after a successful sign-in. Following a deep link into the
app while signed out therefore lands on the tracker list rather than the page that was asked for.
The cookie handler already puts the original path in `?ReturnUrl=`; the login form does not carry it
through. Minor, and only noticeable on a session expiry mid-navigation.

- [ ] Round-trip `ReturnUrl` through the login form and honour it with `LocalRedirect`

---

## E. Documentation

### E1 — Readme describes a repository layout that does not exist *(medium)*

Two concrete mismatches, both in the top section where a new reader starts. (A third — the storage
default — was the same defect as A1 and went away with it.)

| Readme says | Reality |
|---|---|
| `./make.ps1 run` / `./make.ps1 test` | No `make.ps1` anywhere in the repository |
| `test/` holds the xunit projects | Test projects live in `src/`, beside the code |

The `make.ps1` one is the worse of the two: it is the *first* command the Readme gives, under
"Working with it", so the documented entry point into the project does not work. Either restore the
script or replace those blocks with the `dotnet run --project src/Fip.Strive.AppHost` and
`dotnet test src/strive.slnx` they stood for.

- [ ] Restore `make.ps1` or replace it with the underlying `dotnet` commands
- [ ] Correct the `test/` row in the layout table

### E2 — Nothing documents how to run a review of the two solutions *(low)*

`dotnet test src/tracking.slnx` is documented; the equivalent for the platform app, and the fact
that it needs Docker for Testcontainers, is only implied. A short "Working with it" subsection
listing both solutions and their prerequisites would save the next reader a false start.

- [ ] Document both solutions' build/test commands and prerequisites

---

## F. Test coverage

Existing coverage is genuinely good where it exists — roughly 1,800 lines across the storage layer,
the tracking services, and an integration suite that boots the real host against a throwaway
Postgres and asserts that migrations apply and pages render. The gaps are the untested seams:

| Area | State |
|---|---|
| `BlobStore`, `StagingArea`, `StoragePaths` | Covered |
| `Pbkdf2Password`, `AccessGuard` | Covered |
| `TrackerWriter`, `EventRecorder`, `EventReader`, `ExportReader` | Covered |
| Import end-to-end | Covered by `ImportTests` |
| `PackageImporter` unit level | Not covered — no test for the duplicate-name, duplicate-archive or cancellation paths |
| `CatalogReader` | Not covered — including the search behaviour in C6 |
| `TrackerReader` | Not covered |
| `ApiKeyFilter`, `AccessEndpoints`, `/health` | Covered by `Fip.Strive.Tracking.Web.UnitTests` |
| Blazor components (either app) | Not covered — no bUnit project |

The API-and-auth gap was the one that mattered most: the tracker's whole security posture is "the
endpoints require authorization", and nothing asserted it. `Fip.Strive.Tracking.Web.UnitTests` now
does — it boots the real host via `WebApplicationFactory` against a throwaway data directory, so
what it tests is the pipeline as `Program.cs` actually assembles it.

The A2 work is what made this urgent. `/health` is guarded by the same key, and it got there through
a route-handler mapping chosen specifically because the obvious spelling (`AddEndpointFilter` on
`MapHealthChecks`) compiles and silently does nothing. Narrowing the handler's parameters to just
`HttpContext` has the same effect, for the same reason. Either refactor would reopen the endpoint
with no compiler error and no visible symptom — the 401-without-a-key test is the only thing that
would notice.

No Docker or Postgres involved, so it runs wherever `tracking.slnx` runs. It is named `UnitTests`
to match the app's existing test project rather than because it is one; the repo's split is really
"needs infrastructure" against "does not".

Still open: `Fip.Strive.Web.csproj:15` grants `InternalsVisibleTo` to `Fip.Strive.Web.UnitTests`, a
project that does not exist. The platform app has no equivalent web-level suite — its
`IntegrationTests` cover the shell and the import path, but nothing asserts that the health check
reports on Postgres rather than on nothing.

- [x] Add auth/API integration tests for the tracking app *(highest value)*
- [ ] Add unit tests for `PackageImporter`'s duplicate and cancellation paths
- [ ] Add tests for `CatalogReader`, including case-sensitivity of hash search
- [ ] Assert the `postgres` check in `ShellTests`, mirroring the tracker's health test
- [ ] Remove the stale `InternalsVisibleTo`, or create the project it names

---

## Suggested order

Grouped by what they cost against what they buy, rather than strictly by severity.

**First — small, and each closes a real hole**
1. ~~A1 — remove the `E:\` path~~ — **done**
2. E1 — fix the Readme's remaining mismatches
3. B1, B2 — correct `strive.yml`'s filters and triggers
4. ~~F — auth/API integration tests for the tracker~~ — **done**

**Second — bounded work, prevents future damage**
5. C1 — archive expansion limits
6. ~~A2 — real health checks~~ — **done**, but untested (see F)
7. B3, B4 — Dependabot targeting, vulnerable-package scan, CodeQL
8. D1 — schema version stamp

**Third — do before the data gets big**
9. C2 — batch the manifest insert
10. D2 — stop recomputing statistics on every write
11. C6 — search fixes

**Ongoing hygiene**
12. B5 — `.editorconfig`, format check, analyzers
13. A4, A5 — NuGet source hygiene and lock files
14. D3–D7, C3, C5 — the small consistency and robustness items
15. C4 — decide the platform app's authentication posture
16. A3 — Dockerfile for the platform app
