using Microsoft.AspNetCore.Components;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Web.Models;
using TheFipster.ActivityAggregator.Web.Services;

namespace TheFipster.ActivityAggregator.Web.Components.Pages.Import.Tabs;

public partial class InfoTab : ComponentBase
{
    private Dictionary<DataSources, int>? _classifiers;
    private Dictionary<DataSources, int>? _extractors;
    private List<FileCapability> _capabilities = [];

    [Inject]
    public required ScanApi Scanner { get; set; }

    [Inject]
    public required AssimilateApi Assimilater { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        _classifiers = await Scanner.GetClassifiers();
        _extractors = await Assimilater.GetExtractors();

        foreach (var source in Enum.GetValues<DataSources>())
        {
            _capabilities.Add(
                new FileCapability
                {
                    Source = source,
                    ClassifierVersion = _classifiers.TryGetValue(source, out var classifier)
                        ? classifier
                        : null,
                    ExtractorVersion = _extractors.TryGetValue(source, out var extractor)
                        ? extractor
                        : null,
                }
            );
        }

        _capabilities = _capabilities.OrderBy(x => x.Source.ToString()).ToList();
    }
}
