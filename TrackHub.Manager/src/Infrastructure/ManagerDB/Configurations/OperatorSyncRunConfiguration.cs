using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class OperatorSyncRunConfiguration : IEntityTypeConfiguration<OperatorSyncRun>
{
    public void Configure(EntityTypeBuilder<OperatorSyncRun> builder)
    {
        builder.ToTable(name: TableMetadata.OperatorSyncRun, schema: SchemaMetadata.Telemetry);
        builder.Property(x => x.OperatorSyncRunId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.OperatorId).HasColumnName("operatorid");
        builder.Property(x => x.TriggerType).HasColumnName("triggertype");
        builder.Property(x => x.Result).HasColumnName("result");
        builder.Property(x => x.StartedAt).HasColumnName("startedat");
        builder.Property(x => x.CompletedAt).HasColumnName("completedat");
        builder.Property(x => x.DevicesSeen).HasColumnName("devicesseen");
        builder.Property(x => x.DevicesAdded).HasColumnName("devicesadded");
        builder.Property(x => x.DevicesUpdated).HasColumnName("devicesupdated");
        builder.Property(x => x.DevicesRemoved).HasColumnName("devicesremoved");
        builder.Property(x => x.DevicesIgnored).HasColumnName("devicesignored");
        builder.Property(x => x.PositionsRead).HasColumnName("positionsread");
        builder.Property(x => x.PositionsAccepted).HasColumnName("positionsaccepted");
        builder.Property(x => x.PositionsRejected).HasColumnName("positionsrejected");
        builder.Property(x => x.ErrorCode).HasColumnName("errorcode").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.ErrorMessage).HasColumnName("errormessage").HasColumnType(ColumnMetadata.TextField);
        builder.Property(x => x.CorrelationId).HasColumnName("correlationid").HasMaxLength(ColumnMetadata.DefaultNameLength);

        builder.HasIndex(e => new { e.AccountId, e.OperatorId, e.StartedAt });
        builder.HasIndex(e => new { e.AccountId, e.StartedAt });
    }
}
