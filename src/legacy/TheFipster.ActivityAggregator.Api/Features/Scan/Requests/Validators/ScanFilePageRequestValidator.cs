using FluentValidation;
using TheFipster.ActivityAggregator.Domain.Models.Options;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Requests.Validators;

public class ScanFilePageRequestValidator : AbstractValidator<ScanFilePageRequest>
{
    public ScanFilePageRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Page index must be 0 or greater.");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Classified)
            .Must(value => value is null || ClassificationOptions.Items.Contains(value))
            .WithMessage(
                $"Classified value must be null or one of {string.Join(", ", ClassificationOptions.Items)}."
            );

        RuleFor(x => x.Range)
            .Must(value => value is null || RangeOptions.Items.Contains(value))
            .WithMessage(
                $"Range value must be null or one of {string.Join(", ", RangeOptions.Items)}."
            );

        RuleFor(x => x.Search)
            .MinimumLength(3)
            .WithMessage("Search term must have at least 3 characters.");
    }
}
