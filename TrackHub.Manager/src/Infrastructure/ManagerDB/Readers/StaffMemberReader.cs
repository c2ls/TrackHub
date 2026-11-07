using Common.Infrastructure;
using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class StaffMemberReader(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IStaffMemberReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<StaffMemberVm> GetStaffMemberAsync(Guid staffId, CancellationToken cancellationToken)
    {
        var accountId = ResolveAccountScope(null);
        var found = await Context.StaffMembers
            .Where(x => x.StaffId == staffId && (!accountId.HasValue || x.AccountId == accountId.Value))
            .Select(x => new StaffMemberVm(x.StaffId, x.AccountId, x.Name, x.Phone, x.DocumentType, x.DocumentNumber, x.Active, x.EmployeeCode, x.LicenseNumber, x.LicenseExpiresAt, x.DefaultTransporterId, x.StaffType, x.LastModified, x.Version))
            .FirstOrDefaultAsync(cancellationToken);
        ReaderResults.EnsureFound(found, nameof(Entities.StaffMember), staffId.ToString());
        return found;
    }

    public async Task<StaffMembersPageVm> GetStaffMembersByAccountAsync(Guid accountId, string? search, string? staffType, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        var query = Context.StaffMembers.Where(x => x.AccountId == scopedAccountId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = SearchPattern.Contains(search);
            query = query.Where(x => EF.Functions.ILike(x.Name, pattern, SearchPattern.Escape)
                || (x.DocumentNumber != null && EF.Functions.ILike(x.DocumentNumber, pattern, SearchPattern.Escape))
                || (x.EmployeeCode != null && EF.Functions.ILike(x.EmployeeCode, pattern, SearchPattern.Escape)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Name).ThenBy(x => x.StaffId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(x => new StaffMemberVm(x.StaffId, x.AccountId, x.Name, x.Phone, x.DocumentType, x.DocumentNumber, x.Active, x.EmployeeCode, x.LicenseNumber, x.LicenseExpiresAt, x.DefaultTransporterId, x.StaffType, x.LastModified, x.Version))
            .ToListAsync(cancellationToken);
        return new StaffMembersPageVm(items, totalCount);
    }

    /// <summary>
    /// Real assignment rows first (spec 09 §7.2), then the synthesized default-transporter entry — but
    /// only when no active assignment already covers that transporter, so the caller never sees the
    /// same transporter twice.
    /// </summary>
    public async Task<IReadOnlyCollection<StaffAssignmentVm>> GetStaffAssignmentsAsync(Guid staffId, CancellationToken cancellationToken)
    {
        var accountId = ResolveAccountScope(null);
        var staff = await Context.StaffMembers
            .Where(x => x.StaffId == staffId && (!accountId.HasValue || x.AccountId == accountId.Value))
            .Select(x => new { x.StaffId, x.AccountId, x.DefaultTransporterId, x.Active })
            .FirstOrDefaultAsync(cancellationToken);
        ReaderResults.EnsureFound(staff, nameof(Entities.StaffMember), staffId.ToString());

        var now = DateTimeOffset.UtcNow;
        var assignments = await Context.StaffTransporterAssignments
            .Where(x => x.StaffId == staffId
                && x.AccountId == staff.AccountId
                && x.Status == StaffAssignmentStatuses.Active
                && x.StartsAt <= now
                && (x.EndsAt == null || x.EndsAt > now))
            .OrderBy(x => x.StartsAt).ThenBy(x => x.StaffTransporterAssignmentId)
            .Select(x => new StaffAssignmentVm(
                x.StaffId,
                x.AccountId,
                StaffAssignmentResourceTypes.Transporter,
                x.TransporterId.ToString(),
                // An assignment row only counts as usable while the staff themself is active.
                staff.Active,
                x.StartsAt,
                x.EndsAt,
                x.AssignmentType))
            .ToListAsync(cancellationToken);

        if (staff.DefaultTransporterId.HasValue
            && !assignments.Any(x => string.Equals(x.ResourceId, staff.DefaultTransporterId.Value.ToString(), StringComparison.OrdinalIgnoreCase)))
        {
            assignments.Add(new StaffAssignmentVm(staff.StaffId, staff.AccountId, StaffAssignmentResourceTypes.Transporter,
                staff.DefaultTransporterId.Value.ToString(), staff.Active));
        }

        return assignments;
    }

    /// <summary>
    /// Backward-compatible extension (spec 09 §7.2): satisfied by an ACTIVE assignment row or by the
    /// legacy default-transporter check. Existing <c>DocumentAccessPolicy</c> callers keep working and
    /// silently gain the richer semantics. Resource types other than Transporter stay unknown → false.
    /// </summary>
    public async Task<bool> ValidateStaffAssignmentAsync(Guid staffId, string resourceType, string resourceId, CancellationToken cancellationToken)
    {
        if (!string.Equals(resourceType, StaffAssignmentResourceTypes.Transporter, StringComparison.OrdinalIgnoreCase) || !Guid.TryParse(resourceId, out var parsedResourceId))
        {
            return false;
        }

        var accountId = ResolveAccountScope(null);
        var staffAccountId = await Context.StaffMembers
            .Where(x => x.StaffId == staffId && x.Active && (!accountId.HasValue || x.AccountId == accountId.Value))
            .Select(x => (Guid?)x.AccountId)
            .FirstOrDefaultAsync(cancellationToken);

        // An inactive, missing or out-of-scope staff is assigned to nothing.
        if (!staffAccountId.HasValue)
        {
            return false;
        }

        // The transporter must belong to the staff's account too. Writers now reject a cross-account
        // DefaultTransporterId, but rows written before that check existed could otherwise satisfy this
        // validator and hand a staff access to another tenant's documents via DocumentAccessPolicy.
        var matchesDefault = await Context.StaffMembers.AnyAsync(x =>
            x.StaffId == staffId
            && x.DefaultTransporterId == parsedResourceId
            && Context.Transporters.Any(t => t.TransporterId == parsedResourceId && t.AccountId == staffAccountId.Value),
            cancellationToken);

        if (matchesDefault)
        {
            return true;
        }

        var now = DateTimeOffset.UtcNow;
        return await Context.StaffTransporterAssignments.AnyAsync(x =>
            x.StaffId == staffId
            && x.TransporterId == parsedResourceId
            && x.AccountId == staffAccountId.Value
            && x.Status == StaffAssignmentStatuses.Active
            && x.StartsAt <= now
            && (x.EndsAt == null || x.EndsAt > now),
            cancellationToken);
    }

    public async Task<bool> StaffBelongsToAccountAsync(Guid staffId, Guid accountId, CancellationToken cancellationToken)
        => await Context.StaffMembers.AnyAsync(x => x.StaffId == staffId && x.AccountId == accountId, cancellationToken);

    /// <summary>
    /// Staff self-view. The staff id is supplied by the handler from the authenticated principal, so
    /// this can only ever return the caller's own record set (spec 09 AC2).
    /// </summary>
    public async Task<MyStaffProfileVm> GetMyStaffProfileAsync(Guid staffId, CancellationToken cancellationToken)
    {
        // Projected as nullable so a missing staff surfaces as a clean 404 rather than FirstAsync's
        // raw InvalidOperationException.
        var staff = await Context.StaffMembers
            .Where(x => x.StaffId == staffId)
            .Select(x => (StaffMemberVm?)new StaffMemberVm(x.StaffId, x.AccountId, x.Name, x.Phone, x.DocumentType, x.DocumentNumber, x.Active, x.EmployeeCode, x.LicenseNumber, x.LicenseExpiresAt, x.DefaultTransporterId, x.StaffType, x.LastModified, x.Version))
            .FirstOrDefaultAsync(cancellationToken)
            is { } found
            ? found
            : throw new NotFoundException(nameof(Entities.StaffMember), staffId.ToString());

        var qualifications = await Context.StaffQualifications
            .Where(x => x.StaffId == staffId && x.AccountId == staff.AccountId)
            .OrderBy(x => x.ExpiresAt == null).ThenBy(x => x.ExpiresAt).ThenBy(x => x.StaffQualificationId)
            .Select(x => new StaffQualificationVm(x.StaffQualificationId, x.AccountId, x.StaffId, staff.Name,
                x.QualificationType, x.Category, x.Number, x.IssuedAt, x.ExpiresAt, x.IssuingAuthority, x.Status,
                x.DocumentId, x.Notes, x.LastModified))
            .ToListAsync(cancellationToken);

        // The active-assignment predicate (Active + started + not yet ended) is shared with
        // GetStaffAssignmentsAsync and ValidateStaffAssignmentAsync above. Keep the three in step.
        var now = DateTimeOffset.UtcNow;
        var assignments = await (
            from assignment in Context.StaffTransporterAssignments
            join transporter in Context.Transporters on assignment.TransporterId equals transporter.TransporterId
            where assignment.StaffId == staffId
                && assignment.AccountId == staff.AccountId
                && assignment.Status == StaffAssignmentStatuses.Active
                && assignment.StartsAt <= now
                && (assignment.EndsAt == null || assignment.EndsAt > now)
            orderby assignment.StartsAt, assignment.StaffTransporterAssignmentId
            select new StaffTransporterAssignmentVm(assignment.StaffTransporterAssignmentId, assignment.AccountId,
                assignment.StaffId, staff.Name, assignment.TransporterId, transporter.Name, assignment.StartsAt,
                assignment.EndsAt, assignment.AssignmentType, assignment.Status, assignment.CreatedByPrincipal,
                assignment.LastModified)).ToListAsync(cancellationToken);

        return new MyStaffProfileVm(staff, qualifications, assignments);
    }

    public async Task<IReadOnlyCollection<StaffLookupVm>> GetStaffLookupAsync(Guid accountId, string? search, IReadOnlyCollection<Guid>? staffIds, int fetchSize, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        var query = Context.StaffMembers.Where(x => x.AccountId == scopedAccountId);
        if (staffIds is { Count: > 0 })
        {
            var ids = staffIds.ToArray();
            query = query.Where(x => ids.Contains(x.StaffId));
        }
        else
        {
            query = query.Where(x => x.Active);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, SearchPattern.Contains(search), SearchPattern.Escape));
        }

        return await query
            .OrderBy(x => x.Name).ThenBy(x => x.StaffId)
            .Take(fetchSize)
            .Select(x => new StaffLookupVm(x.StaffId, x.Name, x.Active))
            .ToListAsync(cancellationToken);
    }
}
