using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class AlertEventConfiguration : IEntityTypeConfiguration<AlertEvent>
{
    public void Configure(EntityTypeBuilder<AlertEvent> builder)
    {
        builder.ToTable(name: TableMetadata.AlertEvent, schema: SchemaMetadata.Application);
        builder.Property(x => x.AlertEventId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.EventType).HasColumnName("eventtype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Severity).HasColumnName("severity").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.SourceModule).HasColumnName("sourcemodule").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ResourceType).HasColumnName("resourcetype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ResourceId).HasColumnName("resourceid").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.FirstSeenAt).HasColumnName("firstseenat");
        builder.Property(x => x.LastSeenAt).HasColumnName("lastseenat");
        builder.Property(x => x.PayloadJson).HasColumnName("payloadjson").HasColumnType(ColumnMetadata.TextField);
        builder.Property(x => x.DeduplicationKey).HasColumnName("deduplicationkey").HasMaxLength(ColumnMetadata.DefaultTokenLength).IsRequired();
        builder.HasIndex(x => new { x.AccountId, x.Status, x.LastSeenAt });
        builder.HasIndex(x => x.DeduplicationKey);
    }
}
