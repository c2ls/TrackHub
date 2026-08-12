using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class PublicLinkGrantReader(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IPublicLinkGrantReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<PublicLinkGrantVm> GetPublicLinkGrantAsync(Guid publicLinkGrantId, CancellationToken cancellationToken)
    {
        var accountId = ResolveAccountScope(null);
        return await Context.PublicLinkGrants
            .Where(x => x.PublicLinkGrantId == publicLinkGrantId && (!accountId.HasValue || x.AccountId == accountId.Value))
            .Select(x => new PublicLinkGrantVm(x.PublicLinkGrantId, x.AccountId, x.ResourceType, x.ResourceId, x.Scopes, x.Purpose, x.ExpiresAt, x.RevokedAt, x.RevokedBy, x.CreatedByPrincipalId, x.AccessCount, x.LastAccessedAt, x.LastModified, null))
            .FirstAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PublicLinkGrantVm>> GetPublicLinkGrantsByAccountAsync(Guid accountId, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        return await Context.PublicLinkGrants
            .Where(x => x.AccountId == scopedAccountId)
            .OrderByDescending(x => x.LastModified).ThenBy(x => x.PublicLinkGrantId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(x => new PublicLinkGrantVm(x.PublicLinkGrantId, x.AccountId, x.ResourceType, x.ResourceId, x.Scopes, x.Purpose, x.ExpiresAt, x.RevokedAt, x.RevokedBy, x.CreatedByPrincipalId, x.AccessCount, x.LastAccessedAt, x.LastModified, null))
            .ToListAsync(cancellationToken);
    }
}
