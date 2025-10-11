using FluentValidation;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Mediators.Decorators;

public class FilesActionValidator(IFilesAction component, IValidator<ScanFilePageRequest> validator)
    : IFilesAction
{
    public PagedResult<FileIndex> GetFilePage(ScanFilePageRequest request)
    {
        var result = validator.Validate(request);

        if (result.IsValid)
            return component.GetFilePage(request);

        throw new ValidationException(result.Errors);
    }
}
