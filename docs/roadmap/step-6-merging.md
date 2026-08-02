# Step 6 — Merging / unification (L2)

## Goal

Compute the canonical layer: merged, deduplicated, conflict-resolved, sensor-fused data — derived from L1 by explicit, versioned **resolvers**, never hand-edited. This is the hard part of the whole platform (spec §7).

## Design

- **Resolvers**: pluggable, DI-discovered, versioned components (same skeleton as calculators, operating L1→L2). Each declares what it consumes (metric/series types, bucket type, applicability predicate) and emits canonical observations **with lineage** to the L1 observations and its own `(resolverId, version)`.
- **Session grouping is the prerequisite**: cluster session-scoped observations by overlapping time windows (+ sport hints) into candidate **SessionGroups**. Grouping must be reviewable and overridable in the UI — clock drift and near-simultaneous activities make it occasionally ambiguous.
- Canonical values live in the same Postgres/Timescale instance, so replace-by-lineage and cascade invalidation stay single-transaction.

### Fusion taxonomy (build in this order)

1. **Type A — temporal handover**: disjoint source ranges stitched into one continuous series (Polar → Fitbit → Polar eras). Dedup boundary overlaps; prefer a configured source at the seams.
2. **Type B — competing overlap**: both sources claim the same fact (Garmin vs Polar sleep). Keep both in L1; L2 applies **source-priority selection per metric type**, and reports may override the selection. Never collapse destructively.
3. **Type C — session fusion by time sync**: complementary series (HR from one device, GPS from another) aligned on a common start with configurable offset correction.
4. **Type D — session fusion by distance registration**: GPS track without trustworthy timing joined to an HR/speed log by aligning cumulative distances — integrate speed→distance, cumulative haversine over the track, reconcile the two distance axes (linear scale first; monotonic warp/DTW only if real data demands it), interpolate positions per time sample.

- **Source-priority configuration**: per-metric-type priorities, versioned alongside resolvers; time-ranged rules if device eras require it.

## UI

- SessionGroup review: candidate groupings, member files, override (merge/split) controls.
- Conflict inspector: competing-source situations per metric type, current priority, per-value winner with lineage.
- Resolver list with versions and run status.

## Tasks

- [ ] SessionGroup builder + review/override UI
- [ ] Resolver interface + registry + job kind (unit: `(sessionGroupId | dayId, resolverId)`)
- [ ] Canonical storage with lineage; staleness cascade from L1 replays
- [ ] Type A resolver → Type B + source-priority config → Type C → Type D, each validated on the real cases that motivated it
- [ ] Tests per resolver with fixture data (esp. Type D distance registration)

## Done criterion

Each type proves out on its real motivating data: (A) one continuous canonical series across the Polar/Fitbit/Polar eras with clean seams; (B) Garmin-vs-Polar sleep nights where both claims survive in L1 and the priority pick is visible with lineage; (C) an old-watch HR + smartphone GPS run fused on a common clock; (D) an HR/speed log + timing-less GPS track fused by distance registration into one session. A resolver version bump recomputes only its L2 output.

## Out of scope

- Reports/aggregates over canonical data (step 7).
