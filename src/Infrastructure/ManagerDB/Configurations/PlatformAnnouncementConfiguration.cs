using Common.Domain.Constants;
using Common.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class PlatformAnnouncementConfiguration : IEntityTypeConfiguration<PlatformAnnouncement>
{
    /// <summary>Announcement text is plain text with a hard cap; the portal renders it as text, never markup.</summary>
    public const int MessageMaxLength = 500;

    public void Configure(EntityTypeBuilder<PlatformAnnouncement> builder)
    {
        builder.ToTable(
            name: TableMetadata.PlatformAnnouncement,
            schema: SchemaMetadata.Application,
            t => t.HasCheckConstraint("ck_platform_announcements_severity", EnumColumn.Check<AnnouncementSeverity>("severity")));
        builder.Property(x => x.PlatformAnnouncementId).HasColumnName("id");
        builder.Property(x => x.MessageEn).HasColumnName("messageen").HasMaxLength(MessageMaxLength).IsRequired();
        builder.Property(x => x.MessageEs).HasColumnName("messagees").HasMaxLength(MessageMaxLength);
        builder.Property(x => x.Severity).HasColumnName("severity")
            .HasComment(EnumColumn.Comment<AnnouncementSeverity>("Display severity of the announcement."));
        builder.Property(x => x.StartsAt).HasColumnName("startsat");
        builder.Property(x => x.EndsAt).HasColumnName("endsat");
        builder.Property(x => x.Active).HasColumnName("active");
        // Serves the anonymous visibility read (Active + window), the hottest query on this table.
        builder.HasIndex(x => new { x.Active, x.StartsAt, x.EndsAt });
    }
}
