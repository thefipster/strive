# Step 1 — Zip upload & deduplication (L0)

## Goal

Build the content-addressed raw layer: upload vendor takeout ZIPs, unpack them, and catalog every contained file exactly once — no matter how many packages it appears in.

## Design

- **Content addressing by SHA-256** of the file bytes (replaces the previous xxHash3 — L0 identity must be collision-safe, and hashing cost is irrelevant at import frequency).
- **CatalogEntry**: one row per unique blob (hash, size, first-seen). The same hash appearing under many paths in many packages stays one entry.
- **ImportPackage**: one row per uploaded ZIP (archive hash, upload timestamp) plus the full `(pathInArchive → fileHash)` manifest, so every occurrence is recorded.
- **Blob store**: raw bytes on the filesystem, sharded by hash prefix (`ab/cd/abcd…`). Blobs are immutable; identical content is written once.
- **Upload**: drag-and-drop in the UI, streamed to disk (no in-memory buffering; raise Blazor upload limits). Duplicate archives (same archive hash) are rejected/reported, not re-imported.
- **Storage**: catalog tables in PostgreSQL. First EF Core migrations of the new solution.
- Unpacking may run synchronously in this step; it is retrofitted onto the job engine in step 2.

## UI

- Upload page with per-package progress.
- Package list: uploaded archives, file counts, new-vs-known ratio.
- Catalog browser: entries with size, hash, and the packages/paths each blob appeared in.

## Tasks

- [x] Postgres wiring (EF Core, migrations) + `CatalogEntry`, `ImportPackage`, manifest tables
- [x] Streaming upload endpoint/page with progress
- [x] Unpack + SHA-256 + blob-store write with dedup-on-write
- [x] Package list and catalog browser pages

## Done criterion

Upload **two real takeout packages with overlapping content**. The shared files collapse to single catalog entries; the manifests record every path occurrence; re-uploading the same archive is detected as a duplicate and does no work. Verified visually in the catalog browser.

## Out of scope

- Classification, parsing, background jobs (step 2+).

## Result

**Schema** — `catalog_entries` keyed by the content hash itself (no surrogate key, so duplicate
content is impossible by construction), `import_packages` with a unique index on `archive_hash`,
and `package_files` as the manifest with a unique `(package, path)` index. Deleting a package
cascades its manifest but is restricted from removing catalog entries: L0 blobs are immutable and
other packages may still reference them.

**Storage** — `Storage:DataDirectory` (default `data`, relative to the content root) with `blobs/`
and `incoming/` derived from it. Resolved and created at startup, so a bad path fails fast and the
resolved location is in the log. Override with `Storage__DataDirectory`.

**Dedup** — blobs stream to a temp file while hashing, then move into `blobs/ab/cd/<hash>`; if that
path already exists the write is discarded. Blobs land before the database commit, so a failed
import leaves unreferenced bytes that the next attempt deduplicates against rather than a
half-catalogued package. Archives are discarded after import — every file inside is already stored,
so keeping the ZIP too would just double the disk bill.

**Development** — `src/Fip.Strive.AppHost` (Aspire) brings up Postgres with a persistent data
volume and pgAdmin on :8081. Development-time only; the web app takes a plain connection string.

Verified against 13 real Polar Flow activity exports packaged as two overlapping archives, plus
`test/Fip.Strive.IntegrationTests`, which runs the done criterion against real Postgres via
Testcontainers on every CI run.

**Known limits, deferred to step 2:** unpacking runs on the request path and a browser refresh
mid-import abandons it (the job engine fixes this); the manifest is written with EF batch inserts,
which will want `COPY` if a package ever carries six-figure file counts.
