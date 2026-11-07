using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class AccountHolidayConfiguration : IEntityTypeConfiguration<AccountHoliday>
{
    public void Configure(EntityTypeBuilder<AccountHoliday> builder)
    {
        builder.ToTable(name: TableMetadata.AccountHoliday, schema: SchemaMetadata.Application);
        builder.HasKey(x => x.AccountHolidayId);
        builder.Property(x => x.AccountHolidayId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.Date).HasColumnName("date");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.HasIndex(x => new { x.AccountId, x.Date })
            .IsUnique()
            .HasDatabaseName("ux_account_holidays_accountid_date");
    }
}
