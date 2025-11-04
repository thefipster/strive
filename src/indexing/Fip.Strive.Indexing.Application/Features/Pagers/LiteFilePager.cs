using Fip.Strive.Indexing.Application.Infrastructure.Lite.Contexts;
using Fip.Strive.Indexing.Application.Infrastructure.Lite.Repositories;
using Fip.Strive.Indexing.Domain;

namespace Fip.Strive.Indexing.Application.Features.Pagers;

public class LiteFilePager(IndexLiteContext context)
    : LiteSpecificationReader<FileIndex>(context.GetCollection<FileIndex>()) { }
