using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class PublicLinkGrantConfiguration : IEntityTypeConfiguration<PublicLinkGrant>
{
    public void Configure(EntityTypeBuilder<PublicLinkGrant> builder)
    {
        builder.ToTable(name: TableMetadata.PublicLinkGrant, schema: SchemaMetadata.Application);
        builder.Property(x => x.PublicLinkGrantId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.ResourceType).HasColumnName("resourcetype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ResourceId).HasColumnName("resourceid").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Scopes).HasColumnName("scopes").HasMaxLength(ColumnMetadata.DefaultDescriptionLength).IsRequired();
        builder.Property(x => x.Purpose).HasColumnName("purpose").HasMaxLength(ColumnMetadata.DefaultDescriptionLength).IsRequired();
        builder.Property(x => x.SubjectTokenIdHash).HasColumnName("subjecttokenidhash").HasMaxLength(ColumnMetadata.DefaultTokenLength).IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expiresat");
        builder.Property(x => x.RevokedAt).HasColumnName("revokedat");
        builder.Property(x => x.RevokedBy).HasColumnName("revokedby").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.CreatedByPrincipalId).HasColumnName("createdbyprincipalid").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.AccessCount).HasColumnName("accesscount");
        builder.Property(x => x.LastAccessedAt).HasColumnName("lastaccessedat");
        builder.HasIndex(x => x.SubjectTokenIdHash).IsUnique();
        builder.HasIndex(x => new { x.AccountId, x.ResourceType, x.ResourceId });
    }
}
