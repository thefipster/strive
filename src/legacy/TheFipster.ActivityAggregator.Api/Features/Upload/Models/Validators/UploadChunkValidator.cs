using FluentValidation;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Models.Validators;

public class UploadChunkValidator : AbstractValidator<UploadChunkRequest>
{
    public UploadChunkValidator()
    {
        RuleFor(x => x.Chunk).Must(value => value != null).WithMessage("Chunk cannot be null.");

        RuleFor(x => x.TotalSize)
            .GreaterThan(0)
            .WithMessage("Chunk size must be greater than zero.");

        RuleFor(x => x.UploadId)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Upload id cannot be empty or null.");

        RuleFor(x => x.FileName)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("File name cannot be empty or null.");

        RuleFor(x => x.Chunk.Name)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Chunk name cannot be empty or null.");
    }
}
