using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable(name: TableMetadata.NotificationDelivery, schema: SchemaMetadata.Application);
        builder.Property(x => x.NotificationDeliveryId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.NotificationRuleId).HasColumnName("notificationruleid");
        builder.Property(x => x.AlertEventId).HasColumnName("alerteventid");
        builder.Property(x => x.Channel).HasColumnName("channel").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.RecipientPrincipalType).HasColumnName("recipientprincipaltype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Recipient).HasColumnName("recipient").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Attempts).HasColumnName("attempts");
        builder.Property(x => x.ProviderMessageId).HasColumnName("providermessageid").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.Error).HasColumnName("error").HasColumnType(ColumnMetadata.TextField);
        builder.Property(x => x.SentAt).HasColumnName("sentat");
        builder.Property(x => x.ReadAt).HasColumnName("readat");
        builder.Property(x => x.PayloadJson).HasColumnName("payloadjson").HasColumnType(ColumnMetadata.TextField);
        builder.HasIndex(x => new { x.AccountId, x.Status, x.Channel });
        // Dispatcher scan and in-app feed access paths. Explicit names keep the
        // identifiers under PostgreSQL's 63-character limit (the defaults were truncated).
        builder.HasIndex(x => new { x.AccountId, x.Status, x.Created })
            .HasDatabaseName("ix_notification_deliveries_account_status_created");
        builder.HasIndex(x => new { x.RecipientPrincipalType, x.Recipient, x.ReadAt })
            .HasDatabaseName("ix_notification_deliveries_recipient_readat");
    }
}
