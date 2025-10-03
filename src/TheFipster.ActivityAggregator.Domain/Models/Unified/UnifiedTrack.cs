using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Unified;

public class UnifiedTrack
{
    public List<GpsPoint> Points { get; set; } = [];

    public TrackTypes Type { get; set; }
}
