using Common.Application.Interfaces;
using Common.Domain.Helpers;
using TrackHub.Telemetry.Domain.Enums;
using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB.Readers;

public sealed class OperatorSyncRunReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IOperatorSyncRunReader
{
    public async Task<IReadOnlyCollection<OperatorSyncRunVm>> GetAsync(Filters filters, int take, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(take <= 0 ? 50 : take, 1, 500);
        var q = Context.OperatorSyncRuns.AsQueryable();
        q = filters.Apply(q);
        if (!CanAccessAllAccounts && Principal.AccountId.HasValue)
        {
            var acct = Principal.AccountId.Value;
            q = q.Where(x => x.AccountId == acct);
        }
        return await q.OrderByDescending(x => x.StartedAt)
            .Take(pageSize)
            .Select(x => new OperatorSyncRunVm(x.OperatorSyncRunId, x.AccountId, x.OperatorId,
                (SyncTriggerType)x.TriggerType, (OperatorSyncResult)x.Result, x.StartedAt, x.CompletedAt,
                x.DevicesSeen, x.DevicesAdded, x.DevicesUpdated, x.DevicesRemoved, x.DevicesIgnored,
                x.PositionsRead, x.PositionsAccepted, x.PositionsRejected, x.ErrorCode, x.ErrorMessage, x.CorrelationId))
            .ToListAsync(cancellationToken);
    }
}
