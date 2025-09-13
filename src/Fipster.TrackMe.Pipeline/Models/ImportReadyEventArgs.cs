using Fipster.TrackMe.Domain.Models.Indexes;

namespace Fipster.TrackMe.Pipeline.Models;

public class ImportReadyEventArgs(ImportIndex index) : EventArgs
{
    public ImportIndex Index => index;
}
