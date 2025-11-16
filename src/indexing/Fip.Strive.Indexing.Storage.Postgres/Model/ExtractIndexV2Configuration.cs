using Fip.Strive.Indexing.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Indexing.Storage.Postgres.Model;

public class ExtractIndexV2Configuration : IEntityTypeConfiguration<ExtractIndexV2>
{
    public void Configure(EntityTypeBuilder<ExtractIndexV2> builder)
    {
        builder.HasKey(x => x.Filepath);
        builder.HasIndex(x => x.Hash);
        builder.HasIndex(x => x.Kind);
        builder.HasIndex(x => x.Source);
    }
}
