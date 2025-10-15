using TheFipster.ActivityAggregator.Api.Features.Batch.Models;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components.Decorators;

public class AssimilationGroupCombiner(IAssimilationGrouper component, IPessimisticMerger merger)
    : IAssimilationGrouper
{
    public Task<BatchIndex> CombinePerDayAsync(
        InventoryIndex item,
        DataKind kind,
        CancellationToken ct
    )
    {
        return component.CombinePerDayAsync(item, kind, ct);
    }

    public async Task<Dictionary<MergedFile, List<AssimilateIndex>>> CombinePerDayAsync(
        DateTime day,
        CancellationToken ct
    )
    {
        var result = await component.CombinePerDayAsync(day, ct);
        var sessions = result.Where(x => x.Key.Kind == DataKind.Session).ToList();

        if (sessions.Count <= 1)
            return result;

        var groups = GroupOverlappingSessions(sessions);
        UpdateMerge(result, groups, ct);

        return result;
    }

    private List<List<SessionTimespan>> GroupOverlappingSessions(
        List<KeyValuePair<MergedFile, List<AssimilateIndex>>> sessions
    )
    {
        var sorted = sessions
            .Select(x => SessionTimespan.New(x.Key))
            .OrderBy(x => x.Timestamp)
            .ToList();

        var groups = new List<List<SessionTimespan>>();

        while (sorted.Count > 0)
        {
            var firstSampled = GetFirstSampledSession(sorted);
            if (firstSampled == null)
            {
                groups.Add(sorted);
                sorted.Clear();
                continue;
            }

            var overlapping = sorted
                .Where(x =>
                    x.Timestamp > firstSampled.Timestamp.AddMinutes(-30)
                    && x.Timestamp < firstSampled.End
                )
                .ToList();

            if (overlapping.Count > 0)
            {
                groups.Add(overlapping);
                foreach (var session in overlapping)
                    sorted.Remove(session);
            }
        }

        return groups.Where(x => x.Count > 1).ToList();
    }

    private static SessionTimespan? GetFirstSampledSession(List<SessionTimespan> sorted)
    {
        SessionTimespan? firstSampled = null;
        foreach (var session in sorted)
        {
            if (session.Start.HasValue)
            {
                firstSampled = session;
                break;
            }
        }

        return firstSampled;
    }

    private void UpdateMerge(
        Dictionary<MergedFile, List<AssimilateIndex>> mergeResult,
        List<List<SessionTimespan>> groups,
        CancellationToken ct
    )
    {
        foreach (var group in groups)
        {
            var mergeGroup = new Dictionary<MergedFile, List<AssimilateIndex>>();
            foreach (var sessionSpan in group)
                SliceMerge(mergeResult, sessionSpan, mergeGroup);

            AssembleRevisedMerge(mergeResult, ct, mergeGroup);
        }
    }

    private void AssembleRevisedMerge(
        Dictionary<MergedFile, List<AssimilateIndex>> mergeResult,
        CancellationToken ct,
        Dictionary<MergedFile, List<AssimilateIndex>> mergeGroup
    )
    {
        var assimilations = mergeGroup.SelectMany(x => x.Value).ToList();
        var newMerge = merger.CombineAssimilationGroup(
            mergeGroup.Keys.Min(x => x.Timestamp),
            DataKind.Session,
            assimilations,
            ct
        );
        mergeResult.Add(newMerge, assimilations);
    }

    private static void SliceMerge(
        Dictionary<MergedFile, List<AssimilateIndex>> mergeResult,
        SessionTimespan sessionSpan,
        Dictionary<MergedFile, List<AssimilateIndex>> mergeGroup
    )
    {
        var merge = mergeResult.First(x => x.Key.Timestamp == sessionSpan.Timestamp);
        mergeGroup.Add(merge.Key, merge.Value);
        mergeResult.Remove(merge.Key);
    }

    private bool SessionOverlaps(SessionTimespan session, DateTime currentEnd) =>
        session.Start <= currentEnd;
}
