namespace Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;

public interface IZipFileAccess
{
    string Import(string uploadPath);
    // FileStream Open(string importPath);
}
