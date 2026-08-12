using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class DocumentSignatureConfiguration : IEntityTypeConfiguration<DocumentSignature>
{
    private const string TableName = "document_signatures";

    public void Configure(EntityTypeBuilder<DocumentSignature> builder)
    {
        builder.ToTable(name: TableName, schema: SchemaMetadata.Application);
        builder.Property(x => x.DocumentSignatureId).HasColumnName("id");
        builder.Property(x => x.DocumentId).HasColumnName("documentid");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.SignerPrincipalType).HasColumnName("signerprincipaltype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.SignerPrincipalId).HasColumnName("signerprincipalid").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.SignerName).HasColumnName("signername").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.SignatureImageDocumentId).HasColumnName("signatureimagedocumentid");
        builder.Property(x => x.LegalTextAccepted).HasColumnName("legaltextaccepted").HasColumnType(ColumnMetadata.TextField).IsRequired();
        builder.Property(x => x.Latitude).HasColumnName("latitude");
        builder.Property(x => x.Longitude).HasColumnName("longitude");
        builder.Property(x => x.SignedAt).HasColumnName("signedat");
        builder.Property(x => x.CreatedAt).HasColumnName("createdat");
        builder.HasIndex(x => x.DocumentId);
        builder.HasIndex(x => x.AccountId);
    }
}
