using Fip.Strive.Indexing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Indexing.Application.Infrastructure.Postgres.Entities;

public class ZipIndexConfiguration : IEntityTypeConfiguration<ZipIndex>
{
    public void Configure(EntityTypeBuilder<ZipIndex> builder)
    {
        builder.HasKey(x => x.Hash);

        builder.HasIndex(x => x.ReferenceId);

        builder.Property(x => x.Hash).IsRequired();

        builder.Property(x => x.ReferenceId).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.SignalledAt).IsRequired();

        builder.Property(x => x.SignalId).IsRequired();

        builder
            .HasMany(x => x.Files)
            .WithOne()
            .HasForeignKey(x => x.Hash)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Files)
            .WithOne()
            .HasForeignKey(x => x.Hash)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
