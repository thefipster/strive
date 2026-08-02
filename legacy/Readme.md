# Legacy

Two earlier generations of Strive, kept as read-only reference. Neither is part of the root
`strive.sln`, and neither inherits the root build configuration — both carry their own
`Directory.Build.props` / `Directory.Packages.props` so they stay restorable in isolation
without constraining the new solution.

Nothing here is maintained. Code is ported forward deliberately, step by step, per
[the roadmap](../docs/roadmap.md) — never referenced from the new solution.

## `aggregator/`

The original **ActivityAggregator** repo: MAUI app, API, LiteDB storage, and the first
generation of classifiers and extractors.

## `harvester/`

The distributed **Harvester** pipeline: RabbitMQ/Redis, per-stage worker CLIs, Aspire AppHost,
EF Core indexing, and a Blazor Server frontend. Its own `strive.sln` still opens the whole thing.

The distributed runtime is explicitly *not* carried forward. What is worth mining lives in
`harvester/src/ingestion/` — the file classifiers and the format knowledge inside the extractors.
