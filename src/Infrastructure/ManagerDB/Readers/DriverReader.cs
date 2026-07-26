using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class DriverReader(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IDriverReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<DriverVm> GetDriverAsync(Guid driverId, CancellationToken cancellationToken)
    {
        var accountId = ResolveAccountScope(null);
        return await Context.Drivers
            .Where(x => x.DriverId == driverId && (!accountId.HasValue || x.AccountId == accountId.Value))
            .Select(x => new DriverVm(x.DriverId, x.AccountId, x.Name, x.Phone, x.DocumentType, x.DocumentNumber, x.Active, x.EmployeeCode, x.LicenseNumber, x.LicenseExpiresAt, x.DefaultTransporterId, x.LastModified))
            .FirstAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DriverVm>> GetDriversByAccountAsync(Guid accountId, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        return await Context.Drivers
            .Where(x => x.AccountId == scopedAccountId)
            .OrderBy(x => x.Name).ThenBy(x => x.DriverId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(x => new DriverVm(x.DriverId, x.AccountId, x.Name, x.Phone, x.DocumentType, x.DocumentNumber, x.Active, x.EmployeeCode, x.LicenseNumber, x.LicenseExpiresAt, x.DefaultTransporterId, x.LastModified))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Real assignment rows first (spec 09 §7.2), then the synthesized default-transporter entry — but
    /// only when no active assignment already covers that transporter, so the caller never sees the
    /// same transporter twice.
    /// </summary>
    public async Task<IReadOnlyCollection<DriverAssignmentVm>> GetDriverAssignmentsAsync(Guid driverId, CancellationToken cancellationToken)
    {
        var accountId = ResolveAccountScope(null);
        var driver = await Context.Drivers
            .Where(x => x.DriverId == driverId && (!accountId.HasValue || x.AccountId == accountId.Value))
            .Select(x => new { x.DriverId, x.AccountId, x.DefaultTransporterId, x.Active })
            .FirstAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var assignments = await Context.DriverTransporterAssignments
            .Where(x => x.DriverId == driverId
                && x.AccountId == driver.AccountId
                && x.Status == DriverAssignmentStatuses.Active
                && x.StartsAt <= now
                && (x.EndsAt == null || x.EndsAt > now))
            .OrderBy(x => x.StartsAt).ThenBy(x => x.DriverTransporterAssignmentId)
            .Select(x => new DriverAssignmentVm(
                x.DriverId,
                x.AccountId,
                DriverAssignmentResourceTypes.Transporter,
                x.TransporterId.ToString(),
                // An assignment row only counts as usable while the driver themself is active.
                driver.Active,
                x.StartsAt,
                x.EndsAt,
                x.AssignmentType))
            .ToListAsync(cancellationToken);

        if (driver.DefaultTransporterId.HasValue
            && !assignments.Any(x => string.Equals(x.ResourceId, driver.DefaultTransporterId.Value.ToString(), StringComparison.OrdinalIgnoreCase)))
        {
            assignments.Add(new DriverAssignmentVm(driver.DriverId, driver.AccountId, DriverAssignmentResourceTypes.Transporter,
                driver.DefaultTransporterId.Value.ToString(), driver.Active));
        }

        return assignments;
    }

    /// <summary>
    /// Backward-compatible extension (spec 09 §7.2): satisfied by an ACTIVE assignment row or by the
    /// legacy default-transporter check. Existing <c>DocumentAccessPolicy</c> callers keep working and
    /// silently gain the richer semantics. Resource types other than Transporter stay unknown → false.
    /// </summary>
    public async Task<bool> ValidateDriverAssignmentAsync(Guid driverId, string resourceType, string resourceId, CancellationToken cancellationToken)
    {
        if (!string.Equals(resourceType, DriverAssignmentResourceTypes.Transporter, StringComparison.OrdinalIgnoreCase) || !Guid.TryParse(resourceId, out var parsedResourceId))
        {
            return false;
        }

        var accountId = ResolveAccountScope(null);
        var driverAccountId = await Context.Drivers
            .Where(x => x.DriverId == driverId && x.Active && (!accountId.HasValue || x.AccountId == accountId.Value))
            .Select(x => (Guid?)x.AccountId)
            .FirstOrDefaultAsync(cancellationToken);

        // An inactive, missing or out-of-scope driver is assigned to nothing.
        if (!driverAccountId.HasValue)
        {
            return false;
        }

        // The transporter must belong to the driver's account too. Writers now reject a cross-account
        // DefaultTransporterId, but rows written before that check existed could otherwise satisfy this
        // validator and hand a driver access to another tenant's documents via DocumentAccessPolicy.
        var matchesDefault = await Context.Drivers.AnyAsync(x =>
            x.DriverId == driverId
            && x.DefaultTransporterId == parsedResourceId
            && Context.Transporters.Any(t => t.TransporterId == parsedResourceId && t.AccountId == driverAccountId.Value),
            cancellationToken);

        if (matchesDefault)
        {
            return true;
        }

        var now = DateTimeOffset.UtcNow;
        return await Context.DriverTransporterAssignments.AnyAsync(x =>
            x.DriverId == driverId
            && x.TransporterId == parsedResourceId
            && x.AccountId == driverAccountId.Value
            && x.Status == DriverAssignmentStatuses.Active
            && x.StartsAt <= now
            && (x.EndsAt == null || x.EndsAt > now),
            cancellationToken);
    }

    /// <summary>
    /// Driver self-view. The driver id is supplied by the handler from the authenticated principal, so
    /// this can only ever return the caller's own record set (spec 09 AC2).
    /// </summary>
    public async Task<MyDriverProfileVm> GetMyDriverProfileAsync(Guid driverId, CancellationToken cancellationToken)
    {
        // Projected as nullable so a missing driver surfaces as a clean 404 rather than FirstAsync's
        // raw InvalidOperationException.
        var driver = await Context.Drivers
            .Where(x => x.DriverId == driverId)
            .Select(x => (DriverVm?)new DriverVm(x.DriverId, x.AccountId, x.Name, x.Phone, x.DocumentType, x.DocumentNumber, x.Active, x.EmployeeCode, x.LicenseNumber, x.LicenseExpiresAt, x.DefaultTransporterId, x.LastModified))
            .FirstOrDefaultAsync(cancellationToken)
            is { } found
            ? found
            : throw new NotFoundException(nameof(Entities.Driver), driverId.ToString());

        var qualifications = await Context.DriverQualifications
            .Where(x => x.DriverId == driverId && x.AccountId == driver.AccountId)
            .OrderBy(x => x.ExpiresAt == null).ThenBy(x => x.ExpiresAt).ThenBy(x => x.DriverQualificationId)
            .Select(x => new DriverQualificationVm(x.DriverQualificationId, x.AccountId, x.DriverId, driver.Name,
                x.QualificationType, x.Category, x.Number, x.IssuedAt, x.ExpiresAt, x.IssuingAuthority, x.Status,
                x.DocumentId, x.Notes, x.LastModified))
            .ToListAsync(cancellationToken);

        // The active-assignment predicate (Active + started + not yet ended) is shared with
        // GetDriverAssignmentsAsync and ValidateDriverAssignmentAsync above. Keep the three in step.
        var now = DateTimeOffset.UtcNow;
        var assignments = await (
            from assignment in Context.DriverTransporterAssignments
            join transporter in Context.Transporters on assignment.TransporterId equals transporter.TransporterId
            where assignment.DriverId == driverId
                && assignment.AccountId == driver.AccountId
                && assignment.Status == DriverAssignmentStatuses.Active
                && assignment.StartsAt <= now
                && (assignment.EndsAt == null || assignment.EndsAt > now)
            orderby assignment.StartsAt, assignment.DriverTransporterAssignmentId
            select new DriverTransporterAssignmentVm(assignment.DriverTransporterAssignmentId, assignment.AccountId,
                assignment.DriverId, driver.Name, assignment.TransporterId, transporter.Name, assignment.StartsAt,
                assignment.EndsAt, assignment.AssignmentType, assignment.Status, assignment.CreatedByPrincipal,
                assignment.LastModified)).ToListAsync(cancellationToken);

        return new MyDriverProfileVm(driver, qualifications, assignments);
    }
}
