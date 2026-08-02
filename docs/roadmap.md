# Roadmap — Strive Rebuild

This roadmap turns the [wearable data platform spec](wearable-data-platform-spec.md) into an incremental build plan, reusing the existing codebase where it fits. Each step has a detail document and is only ticked off when its **done criterion** has been proven against real data.

## Ground rules

- **Incremental**: one step at a time; the next step starts only when the previous one works as expected.
- **Observable**: every step ships UI to see what is happening and to validate the result.
- **Self-healing**: all processing components (classifiers, readers, extractors, calculators, resolvers) are versioned; a version bump invalidates and replays only the affected data. Data is never mutated by hand.
- **Provenance everywhere**: every stored value traces back to its source file (or input observations) and the component version that produced it.

## Decisions that amend the spec

The spec was written without knowledge of this repository or the homelab. These decisions supersede the corresponding spec sections:

| Topic | Spec says | We decided |
|---|---|---|
| Storage | SQLite (metadata) + Parquet/DuckDB (series), embedded only | **Single PostgreSQL + TimescaleDB instance** for catalog, jobs, provenance, metrics and series. Raw blobs on the filesystem. The homelab provides Postgres and volume backups as a platform feature; a single store makes replace-by-provenance and the invalidation cascade one ACID transaction (resolves spec open question §14.6). |
| Deployment | Single self-contained container | App container + Postgres/Timescale, orchestrated in the homelab. |
| Parser shape | One parser per `(vendor, dataType)` file | **Two-tier extraction**: one *reader* per file format (parses the file once, streams decoded records) fanned out to many small *fact extractors* (one metric/series type each, individually versioned). |
| Prototype migration (§14.7) | Open question | Classifiers and `FileProbe` are ported nearly as-is. Extractors are reshaped into readers + fact extractors, keeping their format knowledge. The distributed pipeline (RabbitMQ/Redis/worker CLIs) is **not** carried forward — everything runs in-process in the Blazor Server app, driven by a job table. |

## Steps

| # | Step | Detail | Status |
|---|---|---|---|
| 0 | Restructure repository, new solution, UI shell | [step-0-restructure.md](roadmap/step-0-restructure.md) | ☑ |
| 1 | Zip upload & deduplication (L0) | [step-1-upload-dedup.md](roadmap/step-1-upload-dedup.md) | ☐ |
| 2 | Job engine & status UI | [step-2-job-engine.md](roadmap/step-2-job-engine.md) | ☐ |
| 3 | File classification | [step-3-classification.md](roadmap/step-3-classification.md) | ☐ |
| 4 | Feature extraction (L1) | [step-4-extraction.md](roadmap/step-4-extraction.md) | ☐ |
| 5 | Computed / derived values | [step-5-derived-values.md](roadmap/step-5-derived-values.md) | ☐ |
| 6 | Merging / unification (L2) | [step-6-merging.md](roadmap/step-6-merging.md) | ☐ |
| 7 | Reports (L3) | [step-7-reports.md](roadmap/step-7-reports.md) | ☐ |

The pipeline steps map onto the spec's layers: step 1 builds L0 (raw), steps 3–5 build L1 (observations), step 6 builds L2 (canonical), step 7 builds L3 (reports). Step 2 is the cross-cutting orchestration every later step rides on.
