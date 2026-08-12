using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class AccountFeatureConfiguration : IEntityTypeConfiguration<AccountFeature>
{
    public void Configure(EntityTypeBuilder<AccountFeature> builder)
    {
        builder.ToTable(name: TableMetadata.AccountFeature, schema: SchemaMetadata.Application);
        builder.Property(x => x.AccountFeatureId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.FeatureKey).HasColumnName("featurekey").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Enabled).HasColumnName("enabled");
        builder.Property(x => x.Tier).HasColumnName("tier").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Source).HasColumnName("source").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.EffectiveFrom).HasColumnName("effectivefrom");
        builder.Property(x => x.EffectiveTo).HasColumnName("effectiveto");
        builder.Property(x => x.ConfigurationJson).HasColumnName("configurationjson").HasColumnType(ColumnMetadata.TextField);
        builder.HasIndex(x => new { x.AccountId, x.FeatureKey }).IsUnique();
    }
}
