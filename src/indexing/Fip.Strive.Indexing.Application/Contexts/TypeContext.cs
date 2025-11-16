using Fip.Strive.Indexing.Domain.Models;

namespace Fip.Strive.Indexing.Application.Contexts;

public class TypeContext
{
    public HashSet<TypeIndexV2> Classifications { get; set; } = [];
}
