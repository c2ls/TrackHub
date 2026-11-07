using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class StaffQualificationConfiguration : IEntityTypeConfiguration<StaffQualification>
{
    public void Configure(EntityTypeBuilder<StaffQualification> builder)
    {
        builder.ToTable(name: TableMetadata.StaffQualification, schema: SchemaMetadata.Application);
        builder.Property(x => x.StaffQualificationId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.StaffId).HasColumnName("staffid");
        builder.Property(x => x.QualificationType).HasColumnName("qualificationtype").HasMaxLength(ColumnMetadata.DefaultFieldLength).IsRequired();
        builder.Property(x => x.Category).HasColumnName("category").HasMaxLength(ColumnMetadata.DefaultFieldLength);
        builder.Property(x => x.Number).HasColumnName("number").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.IssuedAt).HasColumnName("issuedat");
        builder.Property(x => x.ExpiresAt).HasColumnName("expiresat");
        builder.Property(x => x.IssuingAuthority).HasColumnName("issuingauthority").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(ColumnMetadata.DefaultFieldLength).IsRequired();
        builder.Property(x => x.DocumentId).HasColumnName("documentid");
        builder.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(ColumnMetadata.DefaultDescriptionLength);

        builder.HasIndex(x => new { x.AccountId, x.StaffId });
        // Drives the workforce-expiration-scan sweep and the account-wide expirations view.
        builder.HasIndex(x => new { x.AccountId, x.ExpiresAt });
    }
}
