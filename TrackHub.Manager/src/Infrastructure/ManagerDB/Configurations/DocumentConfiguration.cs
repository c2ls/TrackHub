using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable(name: TableMetadata.Document, schema: SchemaMetadata.Application);
        builder.Property(x => x.DocumentId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.OwnerEntityType).HasColumnName("ownerentitytype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.OwnerEntityId).HasColumnName("ownerentityid").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.UploadedByPrincipalType).HasColumnName("uploadedbyprincipaltype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.UploadedByPrincipalId).HasColumnName("uploadedbyprincipalid").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.StorageProvider).HasColumnName("storageprovider").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.StorageKey).HasColumnName("storagekey").HasColumnType(ColumnMetadata.TextField).IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("contenttype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.SizeBytes).HasColumnName("sizebytes");
        builder.Property(x => x.Sha256Hash).HasColumnName("sha256hash").HasMaxLength(ColumnMetadata.DefaultTokenLength).IsRequired();
        builder.Property(x => x.Classification).HasColumnName("classification").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expiresat");
        builder.Property(x => x.VisibilityScope).HasColumnName("visibilityscope").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ScanStatus).HasColumnName("scanstatus").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();

        // Document-library columns.
        builder.Property(x => x.FileName).HasColumnName("filename").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Category).HasColumnName("category").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(ColumnMetadata.DefaultDescriptionLength);
        builder.Property(x => x.CurrentVersion).HasColumnName("currentversion").HasDefaultValue(1);
        builder.Property(x => x.CapturedLatitude).HasColumnName("capturedlatitude");
        builder.Property(x => x.CapturedLongitude).HasColumnName("capturedlongitude");
        builder.Property(x => x.CapturedAtDeviceTime).HasColumnName("capturedatdevicetime");
        builder.Property(x => x.SourceDeviceRegistrationId).HasColumnName("sourcedeviceregistrationid");

        builder.HasIndex(x => new { x.AccountId, x.OwnerEntityType, x.OwnerEntityId });
        builder.HasIndex(x => x.Sha256Hash);
        // Indexes for the expiration scan + per-owner active-type lookup.
        builder.HasIndex(x => new { x.AccountId, x.ExpiresAt, x.Status });
        builder.HasIndex(x => new { x.AccountId, x.OwnerEntityType, x.OwnerEntityId, x.Category, x.Status });
    }
}
