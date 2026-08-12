using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class OperatorHealthCheckConfiguration : IEntityTypeConfiguration<OperatorHealthCheck>
{
    public void Configure(EntityTypeBuilder<OperatorHealthCheck> builder)
    {
        builder.ToTable(name: TableMetadata.OperatorHealthCheck, schema: SchemaMetadata.Telemetry);
        builder.Property(x => x.OperatorHealthCheckId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.OperatorId).HasColumnName("operatorid");
        builder.Property(x => x.CheckType).HasColumnName("checktype");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.LatencyMs).HasColumnName("latencyms");
        builder.Property(x => x.StartedAt).HasColumnName("startedat");
        builder.Property(x => x.CompletedAt).HasColumnName("completedat");
        builder.Property(x => x.ErrorCode).HasColumnName("errorcode").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.ErrorMessage).HasColumnName("errormessage").HasColumnType(ColumnMetadata.TextField);
        builder.Property(x => x.RetryCount).HasColumnName("retrycount").HasDefaultValue(0);
        builder.Property(x => x.CorrelationId).HasColumnName("correlationid").HasMaxLength(ColumnMetadata.DefaultNameLength);

        builder.HasIndex(e => new { e.AccountId, e.OperatorId, e.StartedAt });
    }
}
