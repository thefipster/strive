using TheFipster.ActivityAggregator.Api.Features.Core.Mediators.Contracts;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Core.Mediators.Decorators;

public class ImportHistoryValidator(IImportHistoryAction component) : IImportHistoryAction
{
    public HistoryIndex GetImportHistory(string date)
    {
        if (!DateTime.TryParse(date, out var _))
            throw new ArgumentException(
                "Date is not a valid format. Use ISO 8601 or RFC 3339 format."
            );

        return component.GetImportHistory(date);
    }
}
