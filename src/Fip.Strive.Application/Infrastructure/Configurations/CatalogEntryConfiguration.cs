using Fip.Strive.Application.Features.Catalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Application.Infrastructure.Configurations;

public class CatalogEntryConfiguration : IEntityTypeConfiguration<CatalogEntry>
{
    public void Configure(EntityTypeBuilder<CatalogEntry> builder)
    {
        builder.ToTable("catalog_entries");

        // The content hash is the identity — no surrogate key, so the manifest can reference
        // content directly and duplicates are impossible by construction.
        builder.HasKey(entry => entry.Hash);

        builder.Property(entry => entry.Hash).HasMaxLength(Sha256HexLength);

        builder.Property(entry => entry.SizeBytes).IsRequired();

        builder.Property(entry => entry.FirstSeenUtc).IsRequired();

        builder.HasIndex(entry => entry.FirstSeenUtc);
    }

    private const int Sha256HexLength = 64;
}
