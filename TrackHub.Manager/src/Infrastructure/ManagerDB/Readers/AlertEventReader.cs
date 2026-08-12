using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class AlertEventReader(IApplicationDbContext context, ICurrentPrincipal principal, IVisibleTransporterReader visibleTransporters) : AccountScopedDataAccess(context, principal), IAlertEventReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<IReadOnlyCollection<AlertEventVm>> GetAlertEventsAsync(Guid accountId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        var query = Context.AlertEvents
            .Where(x => x.AccountId == scopedAccountId && (!from.HasValue || x.LastSeenAt >= from) && (!to.HasValue || x.LastSeenAt <= to));

        // Alert-feed visibility follows the source resource: non-privileged users see
        // transporter-mapped events for their group-visible transporters only; events without a
        // group-mappable resource (account-level events such as credential expiry) stay visible to
        // administrators/managers only.
        if (!IsPrivileged)
        {
            if (Principal.PrincipalType != PrincipalType.User || !Principal.UserId.HasValue)
            {
                return [];
            }

            var visibleIds = await visibleTransporters.GetVisibleTransporterIdsAsync(Principal.UserId.Value, scopedAccountId, cancellationToken);
            var visibleKeys = visibleIds.Select(id => id.ToString()).ToList();
            query = query.Where(x => x.ResourceType == "Transporter" && visibleKeys.Contains(x.ResourceId));
        }

        return await query
            .OrderByDescending(x => x.LastSeenAt).ThenBy(x => x.AlertEventId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(x => new AlertEventVm(x.AlertEventId, x.AccountId, x.EventType, x.Severity, x.SourceModule, x.ResourceType, x.ResourceId, x.Status, x.FirstSeenAt, x.LastSeenAt, x.PayloadJson, x.DeduplicationKey, x.LastModified))
            .ToListAsync(cancellationToken);
    }
}
