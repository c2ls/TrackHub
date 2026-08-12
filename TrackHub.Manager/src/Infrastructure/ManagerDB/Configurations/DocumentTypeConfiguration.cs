using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    private const string TableName = "document_types";

    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable(name: TableName, schema: SchemaMetadata.Application);
        builder.Property(x => x.DocumentTypeId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.Category).HasColumnName("category").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.DisplayName).HasColumnName("displayname").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.Required).HasColumnName("required");
        builder.Property(x => x.Expiring).HasColumnName("expiring");
        builder.Property(x => x.DefaultValidityDays).HasColumnName("defaultvaliditydays");
        builder.Property(x => x.Enabled).HasColumnName("enabled");
        builder.Property(x => x.CreatedAt).HasColumnName("createdat");
        // One document-type row per (account, category).
        builder.HasIndex(x => new { x.AccountId, x.Category }).IsUnique();
    }
}
