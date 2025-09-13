using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Models;

namespace Fipster.TrackMe.Web.Models;

public class MergeViewModel
{
    private readonly DateTime date;
    private readonly DateRanges range;

    public MergeViewModel(DateTime date, DateRanges range, IEnumerable<FileExtraction> extracts)
    {
        var fileExtractions = extracts as FileExtraction[] ?? extracts.ToArray();
        if (extracts == null || fileExtractions.Length == 0)
            throw new ArgumentNullException(nameof(extracts));

        this.date = date;
        this.range = range;

        Attributes = fileExtractions.SelectMany(x =>
            x.Attributes.Select(y => new AttributeMerge(x.Source, y.Key, y.Value))
        );
    }

    public IEnumerable<AttributeMerge> Attributes { get; set; }
}
