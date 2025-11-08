using Fip.Strive.Queue.Domain.Models;
using Microsoft.AspNetCore.Components;

namespace Fip.Strive.Harvester.Web.Components.Layout.Components;

public partial class AppBar(QueueMetrics metrics) : ComponentBase
{
    private int _queueCount;
    private double _queueIn;
    private double _queueOut;

    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;

    protected override void OnInitialized()
    {
        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(200));
        _ = RunMetricsLoopAsync(_cts.Token);
    }

    private async Task RunMetricsLoopAsync(CancellationToken ct)
    {
        try
        {
            while (
                !ct.IsCancellationRequested
                && _timer is not null
                && await _timer.WaitForNextTickAsync(ct)
            )
            {
                // Gather metrics off the UI thread if needed
                var inRate = metrics.GetEnqueueRate();
                var jobCount = metrics.GetJobCount();
                var outRate = metrics.GetProcessingRate();

                // Marshal back to UI to update state
                await InvokeAsync(() =>
                {
                    _queueIn = inRate;
                    _queueCount = jobCount;
                    _queueOut = outRate;
                    StateHasChanged();
                });
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
    }
}
