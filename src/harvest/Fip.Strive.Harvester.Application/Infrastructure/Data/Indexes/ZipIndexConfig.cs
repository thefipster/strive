using Fip.Strive.Harvester.Domain.Indexes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Harvester.Application.Infrastructure.Data.Indexes;

public class ZipIndexConfig : IEntityTypeConfiguration<ZipIndex>
{
    public void Configure(EntityTypeBuilder<ZipIndex> builder)
    {
        builder.HasKey(x => x.Filepath);
        builder.HasIndex(x => x.Hash);
    }
}
