using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

/// <summary>
/// Maps <c>OwnerEntityType</c> to the correct visibility primitive. Transporter
/// owners use group visibility; Driver owners use assignment/default-transporter visibility; owner types
/// without a registered resolver are deny-by-default. Also enforces the classification gate.
/// </summary>
public sealed class DocumentAccessPolicy(
    IApplicationDbContext context,
    ICurrentPrincipal principal,
    IVisibleTransporterReader visibleTransporterReader,
    IDriverReader driverReader) : AccountScopedDataAccess(context, principal), IDocumentAccessPolicy
{
    // Scoped service clients have already cleared account + resource/action checks in AuthorizationBehavior;
    // treat them as account-wide for document owner visibility.
    public bool IsPrivilegedPrincipal => IsPrivileged || Principal.PrincipalType == PrincipalType.ServiceClient;

    public async Task<bool> CanAccessOwnerAsync(Guid accountId, string ownerEntityType, string ownerEntityId, bool forWrite, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId, forWrite);

        if (string.Equals(ownerEntityType, DocumentOwnerTypes.Transporter, StringComparison.OrdinalIgnoreCase))
        {
            return await CanAccessTransporterAsync(scopedAccountId, ownerEntityId, cancellationToken);
        }

        if (string.Equals(ownerEntityType, DocumentOwnerTypes.Driver, StringComparison.OrdinalIgnoreCase))
        {
            return await CanAccessDriverOwnerAsync(scopedAccountId, ownerEntityId, cancellationToken);
        }

        // Deny-by-default: owner type without a registered resolver.
        return false;
    }

    public bool IsClearedForClassification(string classification)
        => !DocumentClassifications.IsSensitive(classification) || IsPrivilegedPrincipal;

    public async Task<IReadOnlySet<Guid>> GetVisibleTransporterIdsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);

        if (IsPrivilegedPrincipal)
        {
            var all = await Context.Transporters
                .Where(t => t.AccountId == scopedAccountId)
                .Select(t => t.TransporterId)
                .ToListAsync(cancellationToken);
            return all.ToHashSet();
        }

        if (!Principal.UserId.HasValue)
        {
            return new HashSet<Guid>();
        }

        return await visibleTransporterReader.GetVisibleTransporterIdsAsync(Principal.UserId.Value, scopedAccountId, cancellationToken);
    }

    private async Task<bool> CanAccessTransporterAsync(Guid accountId, string ownerEntityId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(ownerEntityId, out var transporterId))
        {
            return false;
        }

        // Driver principals are assignment-scoped and never receive group visibility.
        if (Principal.PrincipalType == PrincipalType.Driver)
        {
            return Principal.DriverId.HasValue
                && await driverReader.ValidateDriverAssignmentAsync(Principal.DriverId.Value, DocumentOwnerTypes.Transporter, ownerEntityId, cancellationToken);
        }

        if (IsPrivilegedPrincipal)
        {
            return await Context.Transporters.AnyAsync(t => t.TransporterId == transporterId && t.AccountId == accountId, cancellationToken);
        }

        var visible = await GetVisibleTransporterIdsAsync(accountId, cancellationToken);
        return visible.Contains(transporterId);
    }

    private async Task<bool> CanAccessDriverOwnerAsync(Guid accountId, string ownerEntityId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(ownerEntityId, out var driverId))
        {
            return false;
        }

        // A driver may access their own documents.
        if (Principal.PrincipalType == PrincipalType.Driver)
        {
            return Principal.DriverId == driverId;
        }

        // Users/service clients: account + group visibility of the driver's default transporter.
        var driver = await Context.Drivers
            .Where(d => d.DriverId == driverId && d.AccountId == accountId)
            .Select(d => new { d.DefaultTransporterId })
            .FirstOrDefaultAsync(cancellationToken);

        if (driver is null)
        {
            return false;
        }

        if (IsPrivilegedPrincipal)
        {
            return true;
        }

        if (!driver.DefaultTransporterId.HasValue)
        {
            return false;
        }

        var visible = await GetVisibleTransporterIdsAsync(accountId, cancellationToken);
        return visible.Contains(driver.DefaultTransporterId.Value);
    }
}
