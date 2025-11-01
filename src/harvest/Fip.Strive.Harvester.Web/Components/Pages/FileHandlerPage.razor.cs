using Fip.Strive.Core.Ingestion.Contracts;
using Fip.Strive.Core.Ingestion.Services.Contracts;
using Microsoft.AspNetCore.Components;

namespace Fip.Strive.Harvester.Web.Components.Pages;

[Route("/filehandler")]
public partial class FileHandlerPage(IClassifier classifier, IExtractor extractor) : ComponentBase
{
    private Dictionary<IFileClassifier, IFileExtractor?> _handlers = new();

    protected override void OnParametersSet()
    {
        var classifiers = classifier.GetAll().ToArray();
        var extractors = extractor.GetAll().ToArray();

        var map = new Dictionary<IFileClassifier, IFileExtractor?>();

        foreach (var c in classifiers.OrderBy(x => x.Source.ToString()))
        {
            var e = extractors.FirstOrDefault(x => x.Source == c.Source);
            map.Add(c, e);
        }

        _handlers = map;
    }
}
