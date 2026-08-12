using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class TransporterDeviceAssignmentConfiguration : IEntityTypeConfiguration<TransporterDeviceAssignment>
{
    public void Configure(EntityTypeBuilder<TransporterDeviceAssignment> builder)
    {
        builder.ToTable(name: TableMetadata.TransporterDeviceAssignment, schema: SchemaMetadata.Application);
        builder.Property(x => x.TransporterDeviceAssignmentId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.TransporterId).HasColumnName("transporterid");
        builder.Property(x => x.DeviceId).HasColumnName("deviceid");
        builder.Property(x => x.EffectiveFrom).HasColumnName("effectivefrom");
        builder.Property(x => x.EffectiveTo).HasColumnName("effectiveto");
        builder.Property(x => x.Priority).HasColumnName("priority").HasDefaultValue(0);
        builder.Property(x => x.IsPrimary).HasColumnName("isprimary").HasDefaultValue(false);
        builder.Property(x => x.Status).HasColumnName("status").HasDefaultValue(0);
        builder.Property(x => x.AssignmentReason).HasColumnName("assignmentreason").HasMaxLength(ColumnMetadata.DefaultDescriptionLength);
        builder.Property(x => x.CreatedByPrincipalType).HasColumnName("createdbyprincipaltype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired()
            .HasComment("Kind of principal that created the assignment; the principal identity is in CreatedBy.");

        builder
            .HasOne(e => e.Transporter)
            .WithMany(t => t.Assignments)
            .HasForeignKey(e => e.TransporterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(e => e.Device)
            .WithMany(d => d.Assignments)
            .HasForeignKey(e => e.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.AccountId, e.TransporterId, e.EffectiveFrom });
        builder.HasIndex(e => new { e.AccountId, e.DeviceId, e.EffectiveFrom });
        builder.HasIndex(e => new { e.AccountId, e.DeviceId })
            .HasFilter("status = 0")
            .IsUnique()
            .HasDatabaseName("ix_transporter_device_assignments_active_device");
    }
}
