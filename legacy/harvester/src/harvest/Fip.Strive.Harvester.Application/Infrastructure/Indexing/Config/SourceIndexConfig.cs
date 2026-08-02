using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Config;

public class SourceIndexConfig : IEntityTypeConfiguration<SourceIndex>
{
    public void Configure(EntityTypeBuilder<SourceIndex> builder)
    {
        builder.HasKey(x => x.Hash);
        builder.HasIndex(x => x.Filepath);
        builder.HasIndex(x => x.Source);
        builder.HasIndex(x => x.Version);
        builder.HasIndex(x => x.ClassifyHash);
    }
}
