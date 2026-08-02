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

- [ ] Postgres wiring (EF Core, migrations) + `CatalogEntry`, `ImportPackage`, manifest tables
- [ ] Streaming upload endpoint/page with progress
- [ ] Unpack + SHA-256 + blob-store write with dedup-on-write
- [ ] Package list and catalog browser pages

## Done criterion

Upload **two real takeout packages with overlapping content**. The shared files collapse to single catalog entries; the manifests record every path occurrence; re-uploading the same archive is detected as a duplicate and does no work. Verified visually in the catalog browser.

## Out of scope

- Classification, parsing, background jobs (step 2+).
