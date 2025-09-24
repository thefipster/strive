using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components.Activity;

public class UnifiedRecordReader(ActivityContext context)
    : BaseReader<MergedRecord, DateTime>(context);
