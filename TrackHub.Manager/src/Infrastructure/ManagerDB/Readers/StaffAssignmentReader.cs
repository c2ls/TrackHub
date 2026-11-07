using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class StaffAssignmentReader(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IStaffAssignmentReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<StaffAssignmentHistoryPageVm> GetStaffAssignmentHistoryAsync(Guid accountId, Guid? staffId, Guid? transporterId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        var query = Context.StaffTransporterAssignments.Where(x => x.AccountId == scopedAccountId);

        if (staffId.HasValue)
        {
            query = query.Where(x => x.StaffId == staffId.Value);
        }

        if (transporterId.HasValue)
        {
            query = query.Where(x => x.TransporterId == transporterId.Value);
        }

        // Overlap semantics: an assignment is "in the window" when it started before the window closed
        // and had not already ended when the window opened. An open assignment (EndsAt null) always
        // satisfies the lower bound.
        if (to.HasValue)
        {
            query = query.Where(x => x.StartsAt <= to.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.EndsAt == null || x.EndsAt >= from.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await Project(query)
            .Skip(Offset(skip)).Take(PageSize(take))
            .ToListAsync(cancellationToken);
        return new StaffAssignmentHistoryPageVm(items, totalCount);
    }


    /// <summary>
    /// Staff and transporter names are denormalized into the projection so the history table renders
    /// without per-row lookups.
    /// <para>
    /// The ordering lives INSIDE this query, on entity columns, and must stay here: ordering the
    /// projected <see cref="StaffTransporterAssignmentVm"/> afterwards makes EF order by a member of a
    /// constructor-built record struct, which Npgsql cannot translate — it throws at query time. EF
    /// InMemory happily evaluates it client-side, so unit tests do NOT catch the regression.
    /// </para>
    /// </summary>
    private IQueryable<StaffTransporterAssignmentVm> Project(IQueryable<Entities.StaffTransporterAssignment> query)
        // Most recent assignment first.
        => from assignment in query
           join staff in Context.StaffMembers on assignment.StaffId equals staff.StaffId
           join transporter in Context.Transporters on assignment.TransporterId equals transporter.TransporterId
           orderby assignment.StartsAt descending, assignment.StaffTransporterAssignmentId
           select new StaffTransporterAssignmentVm(
               assignment.StaffTransporterAssignmentId,
               assignment.AccountId,
               assignment.StaffId,
               staff.Name,
               assignment.TransporterId,
               transporter.Name,
               assignment.StartsAt,
               assignment.EndsAt,
               assignment.AssignmentType,
               assignment.Status,
               assignment.CreatedByPrincipal,
               assignment.LastModified);
}
