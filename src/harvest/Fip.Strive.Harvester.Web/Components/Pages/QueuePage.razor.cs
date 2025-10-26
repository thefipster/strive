using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fip.Strive.Harvester.Web.Components.Pages;

public partial class QueuePage(IJobReader jobReader)
{
    private bool _dialogVisible;
    private string _selectedError = string.Empty;
    private string _selectedPayload = string.Empty;

    private readonly DialogOptions _dialogOptions = new()
    {
        FullWidth = true,
        MaxWidth = MaxWidth.Large,
        CloseButton = true,
        CloseOnEscapeKey = true,
    };

    [Inject]
    public required IDialogService DialogService { get; set; }

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
        var result = jobReader.GetFailedJobs(state.Page, state.PageSize);
        return Task.FromResult(
            new TableData<JobDetails> { Items = result.Items, TotalItems = result.Total }
        );
    }

    private Task<TableData<JobDetails>> OnRunningRequested(TableState state, CancellationToken ct)
    {
        var result = jobReader.GetJobs(JobStatus.Running, state.Page, state.PageSize);
        return Task.FromResult(
            new TableData<JobDetails> { Items = result.Items, TotalItems = result.Total }
        );
    }

    private void OnOpenErrorDialog(JobDetails job)
    {
        _selectedError = job.Error ?? string.Empty;
        _selectedPayload = job.Payload ?? string.Empty;
        _dialogVisible = true;
        StateHasChanged();
    }
}
