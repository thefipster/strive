using System.Globalization;
using TheFipster.ActivityAggregator.Api.Features.Batch.Components.Contracts;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Formats;
using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components;

public class SeriesMerger : ISeriesMerger
{
    public NormalizedResult Normalize(Dictionary<Parameters, List<string>> series)
    {
        var result = new NormalizedResult();

        if (series.Keys.Contains(Parameters.Timestamp))
            AppendTimeline(result, series);

        if (series.Keys.Contains(Parameters.Latitude) && series.Keys.Contains(Parameters.Longitude))
            AppendTrack(result, series);

        if (series.Keys.Contains(Parameters.Rr))
            AppendPulses(result, series);

        return result;
    }

    private void AppendPulses(NormalizedResult result, Dictionary<Parameters, List<string>> series)
    {
        var pulseSeries = series[Parameters.Rr];
        var pulses = pulseSeries.Select(double.Parse).ToList();

        result.Pulses = pulses;
    }

    private void AppendTrack(NormalizedResult result, Dictionary<Parameters, List<string>> series)
    {
        var track = new UnifiedTrack();

        var latSeries = series[Parameters.Latitude];
        var lonSeries = series[Parameters.Longitude];
        List<string>? altSeries = null;

        if (series.Keys.Contains(Parameters.Altitude))
            altSeries = series[Parameters.Altitude];

        for (int i = 0; i < latSeries.Count; i++)
        {
            var lat = latSeries[i];
            var lon = lonSeries[i];
            string? alt = null;
            if (altSeries != null)
                alt = altSeries[i];

            var point = new GpsPoint(lat, lon, alt);
            track.Points.Add(point);
        }

        result.Track = track;
    }

    private void AppendTimeline(
        NormalizedResult result,
        Dictionary<Parameters, List<string>> series
    )
    {
        var timeseries = series[Parameters.Timestamp];
        var timeline = timeseries.Select(DateTime.Parse).ToList();
        var valuelines = new Dictionary<Parameters, IEnumerable<double>>();

        var valueKeys = series.Keys.Where(x => x != Parameters.Timestamp).ToList();
        foreach (var valueKey in valueKeys)
        {
            var valueSeries = series[valueKey];
            var values = ParseIntoDoubleSeries(valueSeries);
            valuelines.Add(valueKey, values);
        }

        var samples = new UnifiedSamples();
        samples.Timeline = timeline;
        samples.Values = valuelines;
        samples.Start = timeline.Min();
        samples.End = timeline.Max();

        result.Samples = samples;
    }

    private static List<double> ParseIntoDoubleSeries(List<string> valueSeries)
    {
        var values = valueSeries
            .Select(s =>
            {
                if (string.Equals(s, "ECG_QUALITY_HIGH", StringComparison.OrdinalIgnoreCase))
                    return 1.0;

                if (bool.TryParse(s, out var b))
                    return b ? 1.0 : 0.0;

                return double.Parse(s, CultureInfo.InvariantCulture);
            })
            .ToList();
        return values;
    }
}
