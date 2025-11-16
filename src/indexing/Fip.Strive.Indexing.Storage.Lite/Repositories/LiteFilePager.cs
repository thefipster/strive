using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Lite.Contexts;

namespace Fip.Strive.Indexing.Storage.Lite.Repositories;

public class LiteFilePager(IndexLiteContext context)
    : LiteSpecificationReader<FileIndex>(context.GetCollection<FileIndex>()) { }
