namespace TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;

public interface INotifier
{
    Task ReportActionAsync(string action, string message, bool update = false);
    Task ReportProgressAsync(string action, string message, double progress);
}
