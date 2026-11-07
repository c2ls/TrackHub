using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class StaffMemberConfiguration : IEntityTypeConfiguration<StaffMember>
{
    public void Configure(EntityTypeBuilder<StaffMember> builder)
    {
        builder.Property(x => x.Version).HasColumnName("editversion").HasConversion<long>().IsConcurrencyToken();
        builder.ToTable(name: TableMetadata.StaffMember, schema: SchemaMetadata.Application);
        builder.HasKey(x => x.StaffId);
        builder.Property(x => x.StaffId).HasColumnName("id");
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
        builder.Property(x => x.StaffType).HasColumnName("stafftype").HasMaxLength(ColumnMetadata.DefaultFieldLength).IsRequired().HasDefaultValue(StaffTypes.Default);
        builder.HasIndex(x => new { x.AccountId, x.DocumentNumber })
            .IsUnique()
            .HasFilter("documentnumber IS NOT NULL")
            .HasDatabaseName("ux_staff_members_accountid_documentnumber");
    }
}
