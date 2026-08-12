using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class AuditEventReader(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IAuditEventReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<IReadOnlyCollection<AuditEventVm>> GetAuditTrailAsync(Guid accountId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        return await Context.AuditEvents
            .Where(x => x.AccountId == scopedAccountId && (!from.HasValue || x.OccurredAt >= from) && (!to.HasValue || x.OccurredAt <= to))
            .OrderByDescending(x => x.OccurredAt).ThenBy(x => x.AuditEventId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(x => new AuditEventVm(x.AuditEventId, x.AccountId, x.ActorType, x.ActorId, x.Action, x.ResourceType, x.ResourceId, x.Result, x.Reason, x.IpAddress, x.UserAgent, x.CorrelationId, x.OccurredAt))
            .ToListAsync(cancellationToken);
    }
}
