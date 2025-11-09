using System.Collections.Concurrent;
using Fip.Strive.Indexing.Domain.Models;

namespace Fip.Strive.Indexing.Application.Contexts;

public class InventoryContext
{
    public HashSet<DateEntry> Entries { get; set; } = [];
}
