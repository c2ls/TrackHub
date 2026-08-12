using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable(name: TableMetadata.NotificationTemplate, schema: SchemaMetadata.Application);
        builder.Property(x => x.NotificationTemplateId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.TemplateKey).HasColumnName("templatekey").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Channel).HasColumnName("channel").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Locale).HasColumnName("locale").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Subject).HasColumnName("subject").HasMaxLength(ColumnMetadata.DefaultTokenLength);
        builder.Property(x => x.Body).HasColumnName("body").HasColumnType(ColumnMetadata.TextField).IsRequired();
        builder.Property(x => x.Active).HasColumnName("active");
        builder.HasIndex(x => new { x.AccountId, x.TemplateKey, x.Channel, x.Locale }).IsUnique();
    }
}
