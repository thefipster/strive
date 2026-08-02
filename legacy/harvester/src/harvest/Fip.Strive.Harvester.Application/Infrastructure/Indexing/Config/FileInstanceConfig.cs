using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Config;

public class FileInstanceConfig : IEntityTypeConfiguration<FileInstance>
{
    public void Configure(EntityTypeBuilder<FileInstance> builder)
    {
        builder.HasKey(x => x.Filepath);
        builder.HasIndex(x => x.Hash);
        builder.HasIndex(x => x.ParentFilepath);
    }
}
