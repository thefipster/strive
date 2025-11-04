using Fip.Strive.Indexing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Indexing.Application.Infrastructure.Postgres.Entities;

public class DataIndexConfiguration : IEntityTypeConfiguration<DataIndex>
{
    public void Configure(EntityTypeBuilder<DataIndex> builder)
    {
        builder.HasKey(x => x.Hash);

        builder.HasIndex(x => x.ReferenceId);

        builder.Property(x => x.Hash).IsRequired();

        builder.Property(x => x.ReferenceId).IsRequired();

        builder.Property(x => x.ParentId);

        builder
            .HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .HasPrincipalKey(x => x.Hash)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
