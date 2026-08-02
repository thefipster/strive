# Step 3 — File classification

## Goal

Every catalog entry gets classified: which vendor, which data type, which reader/extractors could handle it. Unknowns are first-class citizens, permanently visible in the UI.

## Design

- **Port the legacy classifiers** (61 of them, covering Polar ProTrainer/Flow, RunGPS, GPSies, KML/GPX, Fitbit/Google, TheFipsterApp, Withings, Strava, Garmin, Google Timeline) and the `FileProbe` mechanism from `legacy/harvester/` — they worked well and move nearly as-is behind the new detector interface.
- **Classification is separate from parsing** so it runs cheaply over everything and can be re-run independently.
- A detector looks at cheap signals (path, extension, magic bytes, header peek) and proposes `(vendor, dataType, candidate readers)` with a confidence. Zero or more proposals per entry; ambiguous or unknown entries are flagged.
- **Versioned**: classifiers carry a version; classification results are stored with `(classifierId, version)` provenance. Bumping a classifier re-queues only its work via the step 2 staleness mechanic.
- Runs as jobs: one classification unit per catalog entry.

## UI

- **Coverage matrix** (successor of the legacy `/filehandler` page): sources × classifier/reader availability, so gaps are always apparent.
- **Unknown/unclassified queue**: every entry without a confident classification, browsable — this is where deferred formats (e.g. Withings EKG) surface and wait.
- Catalog browser gains classification filters.

## Tasks

- [ ] Detector interface + DI discovery + version registry
- [ ] Port `FileProbe` and the 61 classifiers; keep their behavior (verified against the seed corpus in `testdata/`)
- [ ] Classification job kind + result storage with provenance
- [ ] Coverage matrix page, unknown queue page, catalog filters
- [ ] Port/extend the legacy classify round-trip tests

## Done criterion

Classify the **full real corpus (~15 GB uncompressed)** via the job engine. Every entry is either confidently classified or visible in the unknown queue; results for the seed corpus match the legacy prototype's classifications; re-running after a classifier version bump reclassifies only that classifier's entries.

## Out of scope

- Extraction of any data (step 4). Classification only tags.
