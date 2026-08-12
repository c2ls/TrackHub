using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class BackgroundJobRunConfiguration : IEntityTypeConfiguration<BackgroundJobRun>
{
    public void Configure(EntityTypeBuilder<BackgroundJobRun> builder)
    {
        builder.ToTable(name: TableMetadata.BackgroundJobRun, schema: SchemaMetadata.Application);
        builder.Property(x => x.BackgroundJobRunId).HasColumnName("id");
        builder.Property(x => x.JobKey).HasColumnName("jobkey").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.ResourceKey).HasColumnName("resourcekey").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.IdempotencyKey).HasColumnName("idempotencykey").HasMaxLength(ColumnMetadata.DefaultTokenLength).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Attempts).HasColumnName("attempts");
        builder.Property(x => x.StartedAt).HasColumnName("startedat");
        builder.Property(x => x.CompletedAt).HasColumnName("completedat");
        builder.Property(x => x.ErrorCode).HasColumnName("errorcode").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.ErrorMessage).HasColumnName("errormessage").HasColumnType(ColumnMetadata.TextField);
        builder.HasIndex(x => new { x.JobKey, x.IdempotencyKey }).IsUnique();
        builder.HasIndex(x => new { x.AccountId, x.StartedAt });
    }
}
