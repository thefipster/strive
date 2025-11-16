using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Indexing.Storage.Postgres.Model;

public class FileIndexConfiguration : IEntityTypeConfiguration<FileIndex>
{
    public void Configure(EntityTypeBuilder<FileIndex> builder)
    {
        builder.HasKey(x => x.Hash);

        builder.HasIndex(x => x.ReferenceId);

        builder.Property(x => x.Hash).IsRequired();

        builder.Property(x => x.ReferenceId).IsRequired();

        builder.Property(x => x.SignalledAt).IsRequired();

        builder.Property(x => x.SignalId).IsRequired();

        builder.Property(x => x.ParentId).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.Classified).IsRequired();

        builder.Property(x => x.ClassificationResult).IsRequired();

        builder
            .HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .HasPrincipalKey(x => x.Hash)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
