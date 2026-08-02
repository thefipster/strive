using FluentValidation;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Models.Validators;

public class ZipsPageValidator : AbstractValidator<ZipsPageRequest>
{
    public ZipsPageValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Page index must be 0 or greater.");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Search)
            .MinimumLength(3)
            .WithMessage("Search term must have at least 3 characters.");
    }
}
