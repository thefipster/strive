using FluentValidation;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators.Decorators;

public class BatchPageValidator(IBatchPageAction component, IValidator<PagedRequest> validator)
    : IBatchPageAction
{
    public PagedResponse<BatchIndex> GetPage(PagedRequest request)
    {
        var result = validator.Validate(request);

        if (result.IsValid)
            return component.GetPage(request);

        throw new ValidationException(result.Errors);
    }
}
