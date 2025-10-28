using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Core.Ingestion.Services.Contracts;
using Fip.Strive.Harvester.Application.Core.Indexing.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Features.Classify.Models;
using Fip.Strive.Harvester.Application.Features.Classify.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Classify.Services;

public class Scanner(IFileIndexer indexer, IClassifier classifier) : IScanner
{
    public Task ClassifyAsync(FileSignal signal, CancellationToken ct)
    {
        var work = WorkItem.FromSignal(signal);

        var index = indexer.Find(work.Signal.Hash);
        if (index == null)
            return Task.CompletedTask;

        var filepath = index.Files.First().Key;
        var result = classifier.Classify(filepath, ct);

        if (result.Count == 1)
        {
            // YAY
        }

        // NAY

        throw new NotImplementedException();
    }
}
