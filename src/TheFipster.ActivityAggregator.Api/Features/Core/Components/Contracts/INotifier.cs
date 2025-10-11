namespace TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;

public interface INotifier
{
    Task ReportAsync(string message, bool update = false);
}
