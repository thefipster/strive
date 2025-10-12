using FluentValidation;
using TheFipster.ActivityAggregator.Domain.Models.Importing;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Models.Validators;

public class AssimilateExtractPageRequestValidator : AbstractValidator<AssimilateExtractPageRequest>
{
    public AssimilateExtractPageRequestValidator()
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

        RuleFor(x => x.Parameter)
            .Must(value => value is null || ParametersOptions.Items.Contains(value))
            .WithMessage(
                $"Parameter value must be null or one of {string.Join(", ", ParametersOptions.Items)}."
            );

        RuleFor(x => x.Date)
            .Must(value => value is null || DateTime.TryParse(value, out _))
            .WithMessage(
                "Date must have a valid format e.g. yyyy-MM-dd or ISO 8601 or RFC 3339 format."
            );
    }
}
