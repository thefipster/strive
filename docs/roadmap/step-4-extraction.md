# Step 4 — Feature extraction (L1)

## Goal

Turn classified files into typed, unit-normalized **observations** (metrics and series) with full provenance, stored in PostgreSQL/TimescaleDB. This step also carries the roadmap's biggest technical risk gate: proving the series schema at RR-interval volume.

## Design

### Two-tier extraction

- **Reader** — one per file format (roughly one per legacy extractor). Opens the file **once** and streams decoded records (trackpoints, sleep phases, JSON nodes) as `IAsyncEnumerable`; never loads whole files. Owns the ugly format knowledge, ported from the legacy extractors.
- **Fact extractor** — many, tiny; each subscribes to a reader's record stream and projects out **one metric or series type** with its provenance locator. Individually versioned.
- The pipeline runs the reader once per file and fans records out to every registered — or every *stale* — fact extractor in a single pass. Per-fact code and versioning without I/O amplification.
- Granularity rule: a fact is one metric/series **type**, not one column. Latitude+longitude is one position fact; values only meaningful together are one fact.

### Data model

- **Buckets**: `Day` (calendar date + timezone) and `Session` (sport, start, end), both hanging off a single explicit `Subject`.
- **Metric**: `(bucketRef, metricTypeId, value, unit, provenanceRef)` — scalar facts.
- **Series**: header `(bucketRef, seriesTypeId, kind ∈ Interval|Period, unit, provenanceRef)` + points. Interval points as `(timestamp|offset, value)`; RR intervals stay irregular `(timestamp, rrMillis)`. Period points as `(from, to, label|value)`.
- **Controlled vocabulary**: curated vendor-independent `MetricType`/`SeriesType` (seeded from the legacy `Parameters` enum, 43 members) with a canonical unit per type; vendor fields and units are normalized during extraction (semicircles→degrees, cm/m, kcal/kJ, …).
- **Provenance**: `(fileHash, readerId, extractorId, extractorVersion, runId, locator)` on every observation. Values are typed, not strings — the legacy stringly-typed `FileExtraction` shape is not carried forward.

### Storage

- Metrics and bucket/vocabulary/provenance tables: regular Postgres tables.
- Series points: **TimescaleDB hypertables** with native compression, chunked by time.
- Replace-by-provenance: re-running an extractor deletes and re-inserts its observations in one transaction.

## Risk gate: RR volume validation

Years of RR data ≈ 50–100k rows/day ⇒ tens to 100+ million rows. As soon as extraction can emit series, feed it the biggest RR/PPI export and measure: insert throughput, compressed chunk size on disk, and representative aggregate queries (`time_bucket` over months). If the schema needs rework, it must happen **before** steps 5–7 build on it.

## UI

- Extraction status per source type (extends the coverage matrix: classified vs extracted).
- Bucket browser: days/sessions with their metrics and series, chart preview per series.
- **Provenance drill-down**: from any displayed value to file, reader/extractor, version, locator.

## Tasks

- [ ] Vocabulary, bucket, metric, series, provenance schema + Timescale hypertables
- [ ] Reader + fact-extractor interfaces, DI discovery, version registry, single-pass fan-out
- [ ] Port legacy extractor format logic into readers; reshape outputs into fact extractors
- [ ] Extraction job kind wired to staleness (per `(catalogEntryId, extractorId)`)
- [ ] RR volume validation run + recorded results
- [ ] Bucket browser + provenance drill-down pages
- [ ] Port/extend the extract round-trip tests against `testdata/`

## Done criterion

The seed corpus extracts to typed observations verifiable in the bucket browser with working provenance drill-down. The **RR volume gate passes**: full-history RR ingest completes, storage lands in expected bounds after compression, and month-scale aggregates return in seconds. An extractor version bump replays only that fact's observations.

## Out of scope

- Values computed *from* observations (step 5), any cross-source merging (step 6).
- New formats without legacy extractors (Garmin FIT, Withings EKG, …) — they remain classified-but-deferred and visible in the unknown/deferred queue.
