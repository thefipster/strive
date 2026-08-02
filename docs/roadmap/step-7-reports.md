# Step 7 — Reports (L3)

## Goal

Pre-built reports and dashboards over canonical data, materialized by background jobs — and the end-to-end proof that the self-healing cascade works from a code fix all the way to a corrected chart.

## Design

- **Reports are jobs**: background jobs compute aggregates over L2 and materialize them as L3. No live/interactive analysis path; dashboards only read L3.
- **Reports are versioned and lineage-aware** like every other component: each materialized report records the L2 lineage and report-code version it was built from, so it is reproducible and precisely invalidated when upstream data heals.
- **Source-parameterized reports** (Type B follow-through): the same report can be built against different source selections (e.g. Garmin-sleep vs Polar-sleep) and is labeled with the selection it used; comparison reports show sources side by side.
- Dashboards: daily metrics, training sessions, HRV/RR-derived trends, source comparisons. Charts follow the project's dataviz conventions.
- Provenance drill-down completes: from a number on a dashboard → L3 report → L2 canonical value → L1 observation(s) → file + locator.

## UI

- Dashboard pages reading L3.
- Report registry: available reports, versions, parameterization, last build, staleness.
- Drill-down from any chart value to its full lineage.

## Tasks

- [ ] L3 storage + report job kind (unit: `(reportId, parameterization)`)
- [ ] Invalidation cascade L2→L3 via lineage
- [ ] First reports: weekly/monthly daily-metric trends, training load overview, HRV trend, one source-comparison report (sleep: Garmin vs Polar)
- [ ] Dashboard pages + drill-down
- [ ] Report reproducibility test (same inputs + version ⇒ identical output)

## Done criterion

**The full self-healing loop closes**: fix a bug in one extractor, bump its version, and watch the cascade — only the affected observations replay (L1), only the dependent derived/canonical values recompute (L1/L2), only the reports built on them rebuild (L3) — with the corrected number visible on the dashboard and every other report untouched. No manual cleanup at any point.

## Out of scope

- New parsers/formats (they slot into steps 3–4 whenever written, e.g. Garmin FIT, Withings EKG).
- Multi-user, auth, real-time analysis (spec non-goals).
