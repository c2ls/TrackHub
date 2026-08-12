using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class GroupVisibilityReader(IApplicationDbContext context, ICurrentPrincipal principal, IVisibleTransporterReader visibleReader) : AccountScopedDataAccess(context, principal), IGroupVisibilityReader
{
    public async Task<bool> ValidateGroupVisibilityAsync(Guid accountId, Guid userId, string resourceType, string resourceId, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);

        if (!CanAccessAllAccounts && Principal.UserId.HasValue && Principal.UserId.Value != userId)
        {
            throw new ForbiddenAccessException();
        }

        if (!Guid.TryParse(resourceId, out var parsedResourceId) || !string.Equals(resourceType, "Transporter", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Reuse the single visibility primitive so the replay check answers identically to the map
        // and its stored fallback — including the Administrator/Manager account-wide bypass
        //.
        var visibleTransporterIds = await visibleReader.GetVisibleTransporterIdsAsync(userId, scopedAccountId, cancellationToken);
        return visibleTransporterIds.Contains(parsedResourceId);
    }
}
