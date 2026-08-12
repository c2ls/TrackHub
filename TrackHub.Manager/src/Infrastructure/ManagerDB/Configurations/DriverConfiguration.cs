using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable(name: TableMetadata.Driver, schema: SchemaMetadata.Application);
        builder.Property(x => x.DriverId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(ColumnMetadata.DefaultPhoneNumberLength);
        builder.Property(x => x.DocumentType).HasColumnName("documenttype").HasMaxLength(ColumnMetadata.DefaultFieldLength);
        builder.Property(x => x.DocumentNumber).HasColumnName("documentnumber").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.Active).HasColumnName("active");
        builder.Property(x => x.EmployeeCode).HasColumnName("employeecode").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.LicenseNumber).HasColumnName("licensenumber").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.LicenseExpiresAt).HasColumnName("licenseexpiresat");
        builder.Property(x => x.DefaultTransporterId).HasColumnName("defaulttransporterid");
        builder.HasIndex(x => new { x.AccountId, x.DocumentNumber });
    }
}
