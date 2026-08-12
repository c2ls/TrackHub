using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class AlertSubscriptionReader(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IAlertSubscriptionReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<IReadOnlyCollection<AlertSubscriptionVm>> GetAlertSubscriptionsAsync(Guid accountId, Guid? principalId, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);

        // Non-privileged callers only see their own subscriptions (self-service filter).
        // Full contact values are intentionally returned: this query backs the subscription editor.
        if (!IsPrivileged)
        {
            principalId = Principal.UserId ?? Principal.DriverId;
        }

        var query = Context.AlertSubscriptions.Where(x => x.AccountId == scopedAccountId);
        if (principalId.HasValue)
        {
            query = query.Where(x => x.PrincipalId == principalId.Value);
        }

        return await query
            .OrderBy(x => x.PrincipalType).ThenBy(x => x.PrincipalId).ThenBy(x => x.AlertSubscriptionId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(x => new AlertSubscriptionVm(x.AlertSubscriptionId, x.AccountId, x.PrincipalType, x.PrincipalId, x.EventTypeFilter, x.Channel, x.Contact, x.Enabled, x.LastModified))
            .ToListAsync(cancellationToken);
    }
}
