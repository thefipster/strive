using FluentValidation;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Mediators.Decorators;

public class ExtractPageValidation(
    IExtractPageAction component,
    IValidator<AssimilateExtractPageRequest> validator
) : IExtractPageAction
{
    public PagedResult<ExtractorIndex> GetExtractPage(AssimilateExtractPageRequest request)
    {
        var result = validator.Validate(request);

        if (result.IsValid)
            return component.GetExtractPage(request);

        throw new ValidationException(result.Errors);
    }
}
