# Step 5 — Computed / derived values

## Goal

Compute per-source derived observations from extracted ones — and in doing so, establish the lineage model that step 6's merge layer will reuse.

## Design

- **Calculators**: versioned, DI-discovered components, structurally like fact extractors, but consuming observations instead of file records. Each declares its input types and produces one derived metric/series type.
- Examples: HRV metrics from RR series, distance by cumulative trapezoidal integration of speed over time, altitude gain/loss from an altitude series, session summaries (max/avg HR) where the source didn't provide them.
- **Per-source only**: a calculator derives within one source's data. Anything combining sources is merging and belongs to step 6.
- **Lineage instead of a file locator**: a derived observation's provenance is *(input observation ids, calculatorId, calculatorVersion, runId)*. This is the first provenance that points at observations rather than bytes — deliberately the same shape L2 canonical values need. Step 5 is the dress rehearsal for step 6's lineage model.
- **Invalidation cascade begins here**: when an upstream extractor replays (step 4 staleness), derived values whose lineage references the replaced observations become stale and recompute. First real test of lineage-driven, precise invalidation.
- Derived observations are stored as L1 observations (same tables), flagged as derived.

## UI

- Calculators list with versions, inputs/outputs, run status.
- Bucket browser shows derived values alongside extracted ones; provenance drill-down follows lineage to the input observations and onward to the source file.

## Tasks

- [ ] Calculator interface + registry + job kind (unit: `(bucketId | seriesId, calculatorId)`)
- [ ] Lineage-capable provenance (observation-ref inputs)
- [ ] Staleness cascade: extractor replay ⇒ dependent calculator units stale
- [ ] First calculators: distance-from-speed integration, basic HRV (e.g. RMSSD daily), session HR summary
- [ ] UI integration + drill-down through lineage
- [ ] Unit tests with known input/output fixtures

## Done criterion

Bump a calculator version: **only** that calculator's derived values are recomputed. Replay an upstream extractor: exactly the derived values depending on its observations go stale and recompute — nothing else. Both verified via the jobs page and drill-down.

## Out of scope

- Cross-source computation, conflict resolution, fusion (step 6).
