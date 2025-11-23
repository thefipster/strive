using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Config;

public class ExtractInstanceConfig : IEntityTypeConfiguration<ExtractIndex>
{
    public void Configure(EntityTypeBuilder<ExtractIndex> builder)
    {
        builder.HasKey(x => x.Hash);
        builder.HasIndex(x => x.Filepath);
        builder.HasIndex(x => x.Source);
        builder.HasIndex(x => x.Version);
        builder.HasMany(x => x.Data).WithOne(x => x.Index).HasForeignKey(x => x.ParentHash);
    }
}
