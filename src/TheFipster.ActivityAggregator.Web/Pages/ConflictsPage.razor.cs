using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Web.Pages;

public partial class ConflictsPage : ComponentBase
{
    private bool loading = true;

    public IEnumerable<UnifyIndex> Conflicts { get; set; } = [];

    protected override async Task OnParametersSetAsync()
    {
        // Conflicts = await Api.GetConflictsAsync();
        loading = false;

        await base.OnParametersSetAsync();
    }
}
