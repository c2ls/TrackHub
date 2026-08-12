using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable(name: TableMetadata.AuditEvent, schema: SchemaMetadata.Application);
        builder.Property(x => x.AuditEventId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.ActorType).HasColumnName("actortype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ActorId).HasColumnName("actorid").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ResourceType).HasColumnName("resourcetype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ResourceId).HasColumnName("resourceid").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Result).HasColumnName("result").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.OldValuesJson).HasColumnName("oldvaluesjson").HasColumnType(ColumnMetadata.TextField);
        builder.Property(x => x.NewValuesJson).HasColumnName("newvaluesjson").HasColumnType(ColumnMetadata.TextField);
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(ColumnMetadata.DefaultDescriptionLength);
        builder.Property(x => x.IpAddress).HasColumnName("ipaddress").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.UserAgent).HasColumnName("useragent").HasMaxLength(ColumnMetadata.DefaultDescriptionLength);
        builder.Property(x => x.CorrelationId).HasColumnName("correlationid").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.OccurredAt).HasColumnName("occurredat");
        builder.HasIndex(x => new { x.AccountId, x.OccurredAt });
        builder.HasIndex(x => x.CorrelationId);
    }
}
