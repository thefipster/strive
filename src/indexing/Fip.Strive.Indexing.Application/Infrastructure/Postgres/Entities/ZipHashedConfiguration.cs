using Fip.Strive.Indexing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Indexing.Application.Infrastructure.Postgres.Entities;

public class ZipHashedConfiguration : IEntityTypeConfiguration<ZipHashed>
{
    public void Configure(EntityTypeBuilder<ZipHashed> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Hash).IsRequired();
    }
}
