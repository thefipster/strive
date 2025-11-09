using Fip.Strive.Indexing.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Indexing.Storage.Postgres.Model;

public class TypeIndexV2Configuration : IEntityTypeConfiguration<TypeIndexV2>
{
    public void Configure(EntityTypeBuilder<TypeIndexV2> builder)
    {
        builder.HasKey(x => x.Hash);
        builder.HasIndex(x => x.Source);
    }
}
