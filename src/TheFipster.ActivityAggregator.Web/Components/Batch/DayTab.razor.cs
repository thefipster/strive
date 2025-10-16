using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Web.Models;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Batch;

public partial class DayTab : ComponentBase
{
    private MergedFile? _mergedFile;
    private HistoryIndex? _history;
    private List<CommonIndex> _indexes = [];

    [Inject]
    public NavigationManager? Navigation { get; set; }

    [Inject]
    public BatchApi? Batcher { get; set; }

    [Inject]
    public HistoryApi? History { get; set; }

    [Parameter]
    public BatchIndex? Batch { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await LoadMergedFile();
        await LoadHistory();
        await base.OnParametersSetAsync();
    }

    private async Task LoadMergedFile()
    {
        if (Batch != null && Batcher != null)
            _mergedFile = await Batcher.GetFileAsync(Batch.Timestamp);
    }

    private async Task LoadHistory()
    {
        if (Batch != null && History != null)
        {
            _indexes.Clear();
            _history = await History.GetImportHistory(Batch.Timestamp);
            _indexes.Add(
                new CommonIndex
                {
                    Type = "batch",
                    Timestamp = _history.Batch.Timestamp,
                    Path = _history.Batch.Filepath,
                    TimeType = _history.Batch.Kind.ToString(),
                    IndexedAt = _history.Batch.IndexedAt,
                    Hash = _history.Batch.Hash,
                }
            );

            foreach (var assimilation in _history.Assimilations)
            {
                _indexes.Add(
                    new CommonIndex
                    {
                        Type = "assimilation",
                        Timestamp = assimilation.Timestamp,
                        Path = assimilation.Path,
                        TimeType = assimilation.Kind.ToString(),
                        IndexedAt = assimilation.IndexedAt,
                        Hash = assimilation.Hash,
                    }
                );
            }

            foreach (var extraction in _history.Extractions)
            {
                _indexes.Add(
                    new CommonIndex
                    {
                        Type = "extraction",
                        TimeType = extraction.Range?.ToString() ?? string.Empty,
                        Path = extraction.Path,
                        Timestamp = extraction.Timestamp,
                        IndexedAt = extraction.IndexedAt,
                        Hash = extraction.ValueHash.Substring(0, 16),
                    }
                );
            }

            foreach (var file in _history.Files)
            {
                _indexes.Add(
                    new CommonIndex
                    {
                        Type = "file",
                        TimeType = file.Range?.ToString() ?? string.Empty,
                        Path = file.Path,
                        Hash = file.Hash,
                        Timestamp = file.Timestamp,
                        IndexedAt = file.IndexedAt,
                    }
                );
            }

            foreach (var zip in _history.Zips)
            {
                _indexes.Add(
                    new CommonIndex
                    {
                        Type = "zip",
                        Hash = zip.Hash,
                        Path = zip.ZipPath,
                        TimeType = nameof(DateRanges.AllTime),
                        IndexedAt = zip.IndexedAt,
                        Timestamp = null,
                    }
                );
            }
        }
    }

    private void OnBackClicked()
    {
        if (Navigation != null && Batch != null)
            Navigation.NavigateTo("import?tab=batch&year=" + Batch.Timestamp.Year);
    }
}
