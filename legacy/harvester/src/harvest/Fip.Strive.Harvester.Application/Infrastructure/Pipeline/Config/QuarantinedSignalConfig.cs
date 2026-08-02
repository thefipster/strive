using Fip.Strive.Harvester.Application.Infrastructure.Pipeline.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Harvester.Application.Infrastructure.Pipeline.Config;

public class QuarantinedSignalConfig : IEntityTypeConfiguration<QuarantinedSignal>
{
    public void Configure(EntityTypeBuilder<QuarantinedSignal> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.CreatedUtc).HasColumnType("timestamp with time zone");
        builder.Property(x => x.Payload).HasColumnType("jsonb");
    }
}
