using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Config;

public class DataIndexConfig : IEntityTypeConfiguration<DataIndex>
{
    public void Configure(EntityTypeBuilder<DataIndex> builder)
    {
        builder.HasKey(x => x.Filepath);
        builder.HasIndex(x => x.Hash);
        builder.HasIndex(x => x.ParentFilepath);
        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => x.Source);
        builder.HasIndex(x => x.IsDay);
    }
}
