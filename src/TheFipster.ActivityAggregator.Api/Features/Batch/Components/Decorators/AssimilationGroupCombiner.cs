using Google.Protobuf.WellKnownTypes;
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

        if (sessions.Count == 0)
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
            .OrderBy(x => x.Start)
            .ToList();

        var groups = new List<List<SessionTimespan>>();
        var currentGroup = new List<SessionTimespan> { sorted[0] };

        var currentEnd = sorted[0].End;

        for (int i = 1; i < sorted.Count; i++)
        {
            var session = sorted[i];

            if (SessionOverlaps(session, currentEnd))
            {
                currentGroup.Add(session);
                currentEnd = new[] { currentEnd, session.End }.Max();
            }
            else
            {
                groups.Add(currentGroup);
                currentGroup = [session];
                currentEnd = session.End;
            }
        }

        groups.Add(currentGroup);
        return groups.Where(x => x.Count > 1).ToList();
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
