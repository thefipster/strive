using FluentValidation;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Mediators.Decorators;

public class ZipsActionValidator(IZipsAction component, IValidator<ZipsPageRequest> validator)
    : IZipsAction
{
    public PagedResponse<ZipIndex> GetZipFilePage(ZipsPageRequest request)
    {
        var result = validator.Validate(request);

        if (result.IsValid)
            return component.GetZipFilePage(request);

        throw new ValidationException(result.Errors);
    }
}
