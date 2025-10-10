namespace TheFipster.ActivityAggregator.Api.Components.Contracts;

public interface INotifier
{
    Task ReportAsync(string message, bool update = false);
}
