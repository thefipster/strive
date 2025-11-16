using Fip.Strive.Harvester.Domain.Indexes;

namespace Fip.Strive.Harvester.Application.Core.Indexing.Contracts;

public interface IFullIndexer<TIndex> : ICheckExistance, IIndexSetter<TIndex>;
