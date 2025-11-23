using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;

namespace Fip.Strive.Harvester.Pipeline.Scanner.Cli.Services;

public interface IScanIndexer : ICheckExistance, ISetNameIndex<FileInstance>;
