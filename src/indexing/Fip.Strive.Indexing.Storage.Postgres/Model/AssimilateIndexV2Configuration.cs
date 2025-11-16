using Fip.Strive.Indexing.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Indexing.Storage.Postgres.Model;

public class AssimilateIndexV2Configuration : IEntityTypeConfiguration<AssimilateIndexV2>
{
    public void Configure(EntityTypeBuilder<AssimilateIndexV2> builder)
    {
        builder.HasKey(x => x.Hash);
        builder.HasIndex(x => x.Source);
    }
}
