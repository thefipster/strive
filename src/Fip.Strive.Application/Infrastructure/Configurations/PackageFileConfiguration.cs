using Fip.Strive.Application.Features.Catalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Application.Infrastructure.Configurations;

public class PackageFileConfiguration : IEntityTypeConfiguration<PackageFile>
{
    public void Configure(EntityTypeBuilder<PackageFile> builder)
    {
        builder.ToTable("package_files");

        builder.HasKey(file => file.Id);

        builder.Property(file => file.PathInArchive).HasMaxLength(1024).IsRequired();

        builder.Property(file => file.Hash).HasMaxLength(Sha256HexLength);

        // A path occurs at most once per archive; the same content may occur at many paths.
        builder.HasIndex(file => new { file.PackageId, file.PathInArchive }).IsUnique();

        // Drives the catalog browser's "which packages did this blob appear in" lookup.
        builder.HasIndex(file => file.Hash);

        builder
            .HasOne(file => file.Package)
            .WithMany(package => package.Files)
            .HasForeignKey(file => file.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Catalog entries outlive the packages that introduced them: L0 blobs are immutable, so
        // deleting a package must never orphan or remove content other packages still reference.
        builder
            .HasOne(file => file.Entry)
            .WithMany(entry => entry.Occurrences)
            .HasForeignKey(file => file.Hash)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private const int Sha256HexLength = 64;
}
