using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Config;

public class ZipIndexConfig : IEntityTypeConfiguration<ZipIndex>
{
    public void Configure(EntityTypeBuilder<ZipIndex> builder)
    {
        builder.HasKey(x => x.Filepath);
        builder.HasIndex(x => x.Hash);
        builder
            .HasMany(x => x.Files)
            .WithOne(x => x.ParentZip)
            .HasForeignKey(x => x.ParentFilepath);
    }
}
