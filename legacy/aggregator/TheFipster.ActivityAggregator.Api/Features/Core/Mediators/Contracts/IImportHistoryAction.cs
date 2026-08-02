using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Core.Mediators.Contracts;

public interface IImportHistoryAction
{
    HistoryIndex GetImportHistory(string date);
}
