using Common.Domain.Constants;
using Common.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

internal class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable(
            name: TableMetadata.Device,
            schema: SchemaMetadata.Application,
            t => t.HasCheckConstraint("ck_devices_devicetypeid", EnumColumn.Check<DeviceType>("devicetypeid")));

        builder.Property(x => x.DeviceId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Identifier).HasColumnName("identifier").IsRequired();
        builder.Property(x => x.Serial).HasColumnName("serial").HasMaxLength(ColumnMetadata.DefaultFieldLength).IsRequired();
        builder.Property(x => x.DeviceTypeId).HasColumnName("devicetypeid")
            .HasComment(EnumColumn.Comment<DeviceType>("Hardware class of the tracked device."));
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(ColumnMetadata.DefaultDescriptionLength);
        builder.Property(x => x.ProviderDisplayName).HasColumnName("providerdisplayname").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.ProviderMetadataHash).HasColumnName("providermetadatahash").HasMaxLength(ColumnMetadata.DefaultTokenLength);
        builder.Property(x => x.ProviderStatus).HasColumnName("providerstatus").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.DetectedStatus).HasColumnName("detectedstatus").HasDefaultValue(0);
        builder.Property(x => x.OperatorId).HasColumnName("operatorid");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.FirstSeenAt).HasColumnName("firstseenat");
        builder.Property(x => x.LastSeenAt).HasColumnName("lastseenat");
        builder.Property(x => x.LastSyncedAt).HasColumnName("lastsyncedat");
        builder.Property(x => x.LastAssignedAt).HasColumnName("lastassignedat");
        builder.Property(x => x.IgnoredAt).HasColumnName("ignoredat");

        builder
            .HasOne(e => e.Operator)
            .WithMany(e => e.Devices)
            .HasForeignKey(e => e.OperatorId);

        builder
            .HasOne(e => e.Account)
            .WithMany(a => a.Devices)
            .HasForeignKey(e => e.AccountId)
            .IsRequired();

        builder.HasIndex(e => e.AccountId);
        builder.HasIndex(e => new { e.AccountId, e.OperatorId, e.Identifier }).IsUnique();
        builder.HasIndex(e => new { e.AccountId, e.OperatorId, e.DetectedStatus, e.LastSyncedAt });
    }
}
