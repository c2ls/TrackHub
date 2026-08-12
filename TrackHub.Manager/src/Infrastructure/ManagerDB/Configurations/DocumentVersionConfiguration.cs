using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    // Table name is a literal (no TrackHubCommon repack).
    private const string TableName = "document_versions";

    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable(name: TableName, schema: SchemaMetadata.Application);
        builder.Property(x => x.DocumentVersionId).HasColumnName("id");
        builder.Property(x => x.DocumentId).HasColumnName("documentid");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.VersionNumber).HasColumnName("versionnumber");
        builder.Property(x => x.StorageProvider).HasColumnName("storageprovider").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.StorageKey).HasColumnName("storagekey").HasColumnType(ColumnMetadata.TextField).IsRequired();
        builder.Property(x => x.Sha256Hash).HasColumnName("sha256hash").HasMaxLength(ColumnMetadata.DefaultTokenLength).IsRequired();
        builder.Property(x => x.SizeBytes).HasColumnName("sizebytes");
        builder.Property(x => x.ContentType).HasColumnName("contenttype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.FileName).HasColumnName("filename").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ScanStatus).HasColumnName("scanstatus").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ReplacedByPrincipalType).HasColumnName("replacedbyprincipaltype").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.ReplacedByPrincipalId).HasColumnName("replacedbyprincipalid").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(ColumnMetadata.DefaultDescriptionLength);
        builder.Property(x => x.CreatedAt).HasColumnName("createdat");
        builder.Property(x => x.BytesPurgedAt).HasColumnName("bytespurgedat");
        builder.HasIndex(x => new { x.DocumentId, x.VersionNumber }).IsUnique();
        builder.HasIndex(x => x.AccountId);
    }
}
