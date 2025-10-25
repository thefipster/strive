using Fip.Strive.Core.Domain.Schemas.Index.Models;

namespace Fip.Strive.Harvester.Application.Features.Import.Repositories.Contracts;

public interface IZipIndexer
{
    ZipIndex? Find(string hash);
    void Upsert(ZipIndex index);
}
