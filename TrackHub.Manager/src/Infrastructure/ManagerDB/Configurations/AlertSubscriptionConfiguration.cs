using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class AlertSubscriptionConfiguration : IEntityTypeConfiguration<AlertSubscription>
{
    public void Configure(EntityTypeBuilder<AlertSubscription> builder)
    {
        builder.ToTable(name: TableMetadata.AlertSubscription, schema: SchemaMetadata.Application);
        builder.Property(x => x.AlertSubscriptionId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.PrincipalType).HasColumnName("principaltype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.PrincipalId).HasColumnName("principalid");
        builder.Property(x => x.EventTypeFilter).HasColumnName("eventtypefilter").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.Channel).HasColumnName("channel").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Contact).HasColumnName("contact").HasMaxLength(ColumnMetadata.DefaultTokenLength);
        builder.Property(x => x.Enabled).HasColumnName("enabled");
        // Explicit name keeps the identifier under PostgreSQL's 63-character limit (the default
        // 5-column name was truncated).
        builder.HasIndex(x => new { x.AccountId, x.PrincipalType, x.PrincipalId, x.EventTypeFilter, x.Channel })
            .IsUnique()
            .HasDatabaseName("ix_alert_subscriptions_principal_filter_channel");
        builder.HasIndex(x => new { x.AccountId, x.EventTypeFilter, x.Enabled });
    }
}
