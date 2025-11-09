using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Indexing.Storage.Postgres.Model;

internal class ZipHashedConfiguration : IEntityTypeConfiguration<ZipHashed>
{
    public void Configure(EntityTypeBuilder<ZipHashed> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Hash).IsRequired();

        builder
            .HasOne(x => x.Zip)
            .WithMany(x => x.Files)
            .HasForeignKey(x => x.Hash)
            .HasPrincipalKey(x => x.Hash)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
