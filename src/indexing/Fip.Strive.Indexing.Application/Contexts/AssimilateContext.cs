using Fip.Strive.Indexing.Domain.Models;

namespace Fip.Strive.Indexing.Application.Contexts;

public class AssimilateContext
{
    public HashSet<AssimilateIndexV2> Assimilations { get; set; } = [];
}
