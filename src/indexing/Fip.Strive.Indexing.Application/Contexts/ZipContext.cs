using Fip.Strive.Indexing.Domain.Models;

namespace Fip.Strive.Indexing.Application.Contexts;

public class ZipContext
{
    public HashSet<ZipIndexV2> Archives { get; set; } = [];
}
