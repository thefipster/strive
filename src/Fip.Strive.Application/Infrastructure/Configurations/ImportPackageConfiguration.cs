using Fip.Strive.Application.Features.Catalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Application.Infrastructure.Configurations;

public class ImportPackageConfiguration : IEntityTypeConfiguration<ImportPackage>
{
    public void Configure(EntityTypeBuilder<ImportPackage> builder)
    {
        builder.ToTable("import_packages");

        builder.HasKey(package => package.Id);

        builder.Property(package => package.ArchiveHash).HasMaxLength(Sha256HexLength);

        // Re-uploading an archive must be detected, not repeated. The database enforces it so a
        // race can never produce two packages for the same bytes.
        builder.HasIndex(package => package.ArchiveHash).IsUnique();

        builder.Property(package => package.FileName).HasMaxLength(512).IsRequired();

        builder.HasIndex(package => package.UploadedUtc);
    }

    private const int Sha256HexLength = 64;
}
