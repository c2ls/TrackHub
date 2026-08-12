using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Telemetry.Domain.Enums;
using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Domain.Records;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Entities;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB.Writers;

public sealed class OperatorSyncRunWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IOperatorSyncRunWriter
{
    public async Task<OperatorSyncRunVm> RecordAsync(OperatorSyncRunDto dto, CancellationToken cancellationToken)
    {
        var scoped = RequireAccountAccess(dto.AccountId);
        var operatorAccountId = await Context.Operators
            .Where(o => o.OperatorId == dto.OperatorId)
            .Select(o => (Guid?)o.AccountId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Operator), dto.OperatorId.ToString());
        if (operatorAccountId != scoped)
        {
            throw new ForbiddenAccessException();
        }

        var entity = new OperatorSyncRun(scoped, dto.OperatorId, (int)dto.TriggerType, (int)dto.Result, dto.StartedAt)
        {
            CompletedAt = dto.CompletedAt,
            DevicesSeen = dto.DevicesSeen,
            DevicesAdded = dto.DevicesAdded,
            DevicesUpdated = dto.DevicesUpdated,
            DevicesRemoved = dto.DevicesRemoved,
            DevicesIgnored = dto.DevicesIgnored,
            PositionsRead = dto.PositionsRead,
            PositionsAccepted = dto.PositionsAccepted,
            PositionsRejected = dto.PositionsRejected,
            ErrorCode = dto.ErrorCode,
            ErrorMessage = dto.ErrorMessage,
            CorrelationId = dto.CorrelationId
        };
        await Context.OperatorSyncRuns.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return new OperatorSyncRunVm(entity.OperatorSyncRunId, entity.AccountId, entity.OperatorId,
            (SyncTriggerType)entity.TriggerType, (OperatorSyncResult)entity.Result, entity.StartedAt, entity.CompletedAt,
            entity.DevicesSeen, entity.DevicesAdded, entity.DevicesUpdated, entity.DevicesRemoved, entity.DevicesIgnored,
            entity.PositionsRead, entity.PositionsAccepted, entity.PositionsRejected, entity.ErrorCode, entity.ErrorMessage, entity.CorrelationId);
    }
}
