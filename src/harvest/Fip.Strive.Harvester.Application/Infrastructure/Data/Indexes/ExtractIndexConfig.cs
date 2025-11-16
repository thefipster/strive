using Fip.Strive.Harvester.Domain.Indexes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Harvester.Application.Infrastructure.Data.Indexes;

public class ExtractInstanceConfig : IEntityTypeConfiguration<ExtractIndex>
{
    public void Configure(EntityTypeBuilder<ExtractIndex> builder)
    {
        builder.HasKey(x => x.Hash);
        builder.HasIndex(x => x.Filepath);
        builder.HasIndex(x => x.Source);
        builder.HasIndex(x => x.Version);
    }
}
