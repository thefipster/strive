using FluentValidation;

namespace TheFipster.ActivityAggregator.Domain.Models.Requests.Validators;

public class PagedRequestValidator : AbstractValidator<PagedRequest>
{
    public PagedRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Page index must be 0 or greater.");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
    }
}
