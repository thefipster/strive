using Fip.Strive.Indexing.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Indexing.Storage.Postgres.Model;

public class FileIndexV2Configuration : IEntityTypeConfiguration<FileIndexV2>
{
    public void Configure(EntityTypeBuilder<FileIndexV2> builder)
    {
        builder.HasKey(x => x.Filepath);
        builder.HasIndex(x => x.Hash);

        builder.Property(x => x.Filepath).HasMaxLength(512);
        builder.Property(x => x.ParentFilepath).HasMaxLength(512);
        builder.Property(x => x.Hash).IsRequired().HasMaxLength(128);
        builder.Property(x => x.ClassificationHash).IsRequired().HasMaxLength(128);
    }
}
