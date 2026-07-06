using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Telemetry.Domain.Enums;
using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Domain.Records;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Entities;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB.Writers;

public sealed class OperatorHealthCheckWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IOperatorHealthCheckWriter
{
    public async Task<OperatorHealthCheckVm> RecordAsync(OperatorHealthCheckDto dto, CancellationToken cancellationToken)
    {
        var scoped = RequireAccountAccess(dto.AccountId);
        // Read-only ownership check against the operator master row (spec 01.3 §5.2).
        var operatorAccountId = await Context.Operators
            .Where(o => o.OperatorId == dto.OperatorId)
            .Select(o => (Guid?)o.AccountId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Operator), dto.OperatorId.ToString());
        if (operatorAccountId != scoped)
        {
            throw new ForbiddenAccessException();
        }

        var entity = new OperatorHealthCheck(
            scoped, dto.OperatorId, (int)dto.CheckType, (int)dto.Status, dto.LatencyMs,
            dto.StartedAt, dto.CompletedAt, dto.ErrorCode, dto.ErrorMessage, dto.RetryCount, dto.CorrelationId);

        await Context.OperatorHealthChecks.AddAsync(entity, cancellationToken);

        // Per the Slice B decision, Telemetry has read-only access to the operator master row and no
        // longer writes the denormalized operator health-summary columns. The operator health/sync
        // summary is derived from the telemetry tables at read time (GetLatestHealthAsync).
        await Context.SaveChangesAsync(cancellationToken);
        return new OperatorHealthCheckVm(entity.OperatorHealthCheckId, entity.AccountId, entity.OperatorId,
            (OperatorHealthCheckType)entity.CheckType, (OperatorHealthStatus)entity.Status, entity.LatencyMs,
            entity.StartedAt, entity.CompletedAt, entity.ErrorCode, entity.ErrorMessage, entity.RetryCount, entity.CorrelationId);
    }
}
