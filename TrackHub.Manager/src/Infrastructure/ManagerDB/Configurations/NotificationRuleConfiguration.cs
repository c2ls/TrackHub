using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class NotificationRuleConfiguration : IEntityTypeConfiguration<NotificationRule>
{
    public void Configure(EntityTypeBuilder<NotificationRule> builder)
    {
        builder.ToTable(name: TableMetadata.NotificationRule, schema: SchemaMetadata.Application);
        builder.Property(x => x.NotificationRuleId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.RuleKey).HasColumnName("rulekey").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.RuleType).HasColumnName("ruletype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Enabled).HasColumnName("enabled");
        builder.Property(x => x.TriggerEvent).HasColumnName("triggerevent").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.RecipientSelector).HasColumnName("recipientselector").HasColumnType(ColumnMetadata.TextField).IsRequired();
        builder.Property(x => x.ChannelsJson).HasColumnName("channelsjson").HasColumnType(ColumnMetadata.TextField).IsRequired();
        builder.Property(x => x.ThrottlingJson).HasColumnName("throttlingjson").HasColumnType(ColumnMetadata.TextField);
        builder.Property(x => x.ConfigurationJson).HasColumnName("configurationjson").HasColumnType(ColumnMetadata.TextField);
        builder.HasIndex(x => new { x.AccountId, x.RuleKey }).IsUnique();
    }
}
