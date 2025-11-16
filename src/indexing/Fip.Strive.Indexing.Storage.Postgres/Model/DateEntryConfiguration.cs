using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Indexing.Storage.Postgres.Model;

public class DateEntryConfiguration : IEntityTypeConfiguration<DateEntry>
{
    public void Configure(EntityTypeBuilder<DateEntry> builder)
    {
        builder.HasKey(x => x.Date);

        builder.HasIndex(x => x.Year);
        builder.HasIndex(x => x.Month);
        builder.HasIndex(x => x.Day);
        builder.HasIndex(x => x.Timestamp);

        builder.Property(x => x.Year).IsRequired();

        builder.Property(x => x.Month).IsRequired();

        builder.Property(x => x.Day).IsRequired();

        builder.Property(x => x.Date).IsRequired();

        builder.Property(x => x.Timestamp).IsRequired();
    }
}
