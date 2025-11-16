using Fip.Strive.Harvester.Domain.Indexes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Harvester.Application.Infrastructure.Data.Indexes;

public class FileInstanceConfig : IEntityTypeConfiguration<FileInstance>
{
    public void Configure(EntityTypeBuilder<FileInstance> builder)
    {
        builder.HasKey(x => x.Filepath);
        builder.HasIndex(x => x.Hash);
        builder.HasIndex(x => x.ParentFilepath);
    }
}
