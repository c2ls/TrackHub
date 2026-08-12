using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class AccountSupportGrantReader(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IAccountSupportGrantReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<AccountSupportGrantVm> GetSupportGrantStatusAsync(Guid accountSupportGrantId, CancellationToken cancellationToken)
    {
        var accountId = ResolveAccountScope(null);
        return await Context.AccountSupportGrants
            .Where(x => x.AccountSupportGrantId == accountSupportGrantId && (!accountId.HasValue || x.AccountId == accountId.Value))
            .Select(x => new AccountSupportGrantVm(x.AccountSupportGrantId, x.AccountId, x.SupportUserId, x.Reason, x.TicketReference, x.ApprovedBy, x.ApprovedAt, x.AccessLevel, x.StartsAt, x.EndsAt, x.RevokedAt, x.RevokedBy, x.LastModified))
            .FirstAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AccountSupportGrantVm>> GetAccountSupportGrantsAsync(Guid? accountId, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = ResolveAccountScope(accountId);
        return await Context.AccountSupportGrants
            .Where(x => !scopedAccountId.HasValue || x.AccountId == scopedAccountId.Value)
            .OrderByDescending(x => x.LastModified).ThenBy(x => x.AccountSupportGrantId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(x => new AccountSupportGrantVm(x.AccountSupportGrantId, x.AccountId, x.SupportUserId, x.Reason, x.TicketReference, x.ApprovedBy, x.ApprovedAt, x.AccessLevel, x.StartsAt, x.EndsAt, x.RevokedAt, x.RevokedBy, x.LastModified))
            .ToListAsync(cancellationToken);
    }
}
