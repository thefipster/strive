using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Indexing.Storage.Postgres.Model;

public class DataIndexConfiguration : IEntityTypeConfiguration<DataIndex>
{
    public void Configure(EntityTypeBuilder<DataIndex> builder)
    {
        builder.HasKey(x => x.Filepath);

        builder.HasIndex(x => x.ReferenceId);

        builder.Property(x => x.Hash).IsRequired();

        builder.Property(x => x.ReferenceId).IsRequired();

        builder.Property(x => x.ParentId);

        builder
            .HasOne(data => data.Parent)
            .WithMany(file => file.Children)
            .HasForeignKey(data => data.ParentId)
            .HasPrincipalKey(file => file.Hash)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
