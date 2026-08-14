namespace Fip.Strive.Application.Features.Jobs.Models;

/// <param name="Note">What is being worked on right now — a file path, for unpacking.</param>
public readonly record struct JobProgress(int Current, int Total, string? Note = null);
