using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Components;

namespace TheFipster.ActivityAggregator.Domain.Models.Unified;

public class UnifiedTrack
{
    public List<GpsPoint> Points { get; set; } = [];

    public TrackTypes Type { get; set; }
}
