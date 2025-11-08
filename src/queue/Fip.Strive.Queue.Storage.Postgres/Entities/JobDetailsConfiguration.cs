using Fip.Strive.Queue.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fip.Strive.Queue.Storage.Postgres.Entities;

public class JobDetailsConfiguration : IEntityTypeConfiguration<JobDetails>
{
    public void Configure(EntityTypeBuilder<JobDetails> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.CreatedAt);
    }
}
