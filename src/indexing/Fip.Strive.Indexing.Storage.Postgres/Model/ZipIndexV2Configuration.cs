using Fip.Strive.Indexing.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Indexing.Storage.Postgres.Model;

public class ZipIndexV2Configuration : IEntityTypeConfiguration<ZipIndexV2>
{
    public void Configure(EntityTypeBuilder<ZipIndexV2> builder)
    {
        builder.HasKey(x => x.Filepath);
        builder.HasIndex(x => x.Hash);
    }
}
