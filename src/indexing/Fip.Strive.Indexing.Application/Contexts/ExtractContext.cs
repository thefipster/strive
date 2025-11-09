using Fip.Strive.Indexing.Domain.Models;

namespace Fip.Strive.Indexing.Application.Contexts;

public class ExtractContext
{
    public HashSet<ExtractIndexV2> Extractions { get; set; } = [];
}
