using Fip.Strive.Application.Features.Catalog.Models;
using Fip.Strive.Application.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Application.Infrastructure;

public class StriveContext(DbContextOptions<StriveContext> options) : DbContext(options)
{
    public DbSet<CatalogEntry> CatalogEntries => Set<CatalogEntry>();

    public DbSet<ImportPackage> ImportPackages => Set<ImportPackage>();

    public DbSet<PackageFile> PackageFiles => Set<PackageFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Registered explicitly rather than by assembly scan: the scan would also have to load
        // the design-time factory, whose dependency is not shipped with the app.
        modelBuilder.ApplyConfiguration(new CatalogEntryConfiguration());
        modelBuilder.ApplyConfiguration(new ImportPackageConfiguration());
        modelBuilder.ApplyConfiguration(new PackageFileConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
