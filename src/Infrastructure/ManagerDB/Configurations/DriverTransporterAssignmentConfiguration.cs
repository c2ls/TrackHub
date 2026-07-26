using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class DriverTransporterAssignmentConfiguration : IEntityTypeConfiguration<DriverTransporterAssignment>
{
    public void Configure(EntityTypeBuilder<DriverTransporterAssignment> builder)
    {
        builder.ToTable(name: TableMetadata.DriverTransporterAssignment, schema: SchemaMetadata.Application);
        builder.Property(x => x.DriverTransporterAssignmentId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.DriverId).HasColumnName("driverid");
        builder.Property(x => x.TransporterId).HasColumnName("transporterid");
        builder.Property(x => x.StartsAt).HasColumnName("startsat");
        builder.Property(x => x.EndsAt).HasColumnName("endsat");
        builder.Property(x => x.AssignmentType).HasColumnName("assignmenttype").HasMaxLength(ColumnMetadata.DefaultFieldLength).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(ColumnMetadata.DefaultFieldLength).IsRequired();
        builder.Property(x => x.CreatedByPrincipal).HasColumnName("createdbyprincipal").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();

        // Explicit names: the auto-generated spellings land at 58 and 63 characters, the latter exactly
        // on PostgreSQL's identifier limit where EF's `~` truncation and Postgres' silent truncation
        // start to diverge (rules.md "Database conventions").
        builder.HasIndex(x => new { x.AccountId, x.DriverId, x.Status })
            .HasDatabaseName("ix_driver_assignments_account_driver_status");
        builder.HasIndex(x => new { x.AccountId, x.TransporterId, x.Status })
            .HasDatabaseName("ix_driver_assignments_account_transporter_status");
    }
}
