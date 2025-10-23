using Fip.Strive.Harvester.Application.Core.Queue.Enums;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Services;
using MudBlazor;

namespace Fip.Strive.Harvester.Web.Components.Pages;

public partial class QueuePage(IJobReader jobReader, QueuedHostedService queue)
{
    private bool _isRunning = true;

    protected override void OnParametersSet()
    {
        _isRunning = queue.IsRunning;
    }

    private Task<TableData<JobDetails>> OnUpcommingRequested(TableState state, CancellationToken ct)
    {
        var result = jobReader.GetUpcommingJobs(state.Page, state.PageSize);
        return Task.FromResult(
            new TableData<JobDetails> { Items = result.Items, TotalItems = result.Total }
        );
    }

    private Task<TableData<JobDetails>> OnSucceededRequested(
        TableState state,
        CancellationToken arg2
    )
    {
        var result = jobReader.GetSucceededJobs(state.Page, state.PageSize);
        return Task.FromResult(
            new TableData<JobDetails> { Items = result.Items, TotalItems = result.Total }
        );
    }

    private Task<TableData<JobDetails>> OnFailedRequested(TableState state, CancellationToken ct)
    {
        var result = jobReader.GetJobs(JobStatus.Running, state.Page, state.PageSize);
        return Task.FromResult(
            new TableData<JobDetails> { Items = result.Items, TotalItems = result.Total }
        );
    }

    private async Task OnPauseRequested()
    {
        await queue.StopWorkAsync();
        _isRunning = false;
    }

    private void OnResumeRequested()
    {
        queue.StartWork();
        _isRunning = true;
    }
}
