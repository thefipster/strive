using Fip.Strive.Indexing.Domain.Models;

namespace Fip.Strive.Indexing.Application.Contexts;

public class FileContext
{
    public HashSet<FileIndexV2> Files { get; set; } = [];
}
