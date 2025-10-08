using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Formats;

namespace TheFipster.ActivityAggregator.Domain.Models.Importing;

public class UnifiedTrack
{
    public List<GpsPoint> Points { get; set; } = [];

    public TrackTypes Type { get; set; }
}
