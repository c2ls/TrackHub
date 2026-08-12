using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class DriverAssignmentReader(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IDriverAssignmentReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<IReadOnlyCollection<DriverTransporterAssignmentVm>> GetDriverAssignmentHistoryAsync(Guid accountId, Guid? driverId, Guid? transporterId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        var query = Context.DriverTransporterAssignments.Where(x => x.AccountId == scopedAccountId);

        if (driverId.HasValue)
        {
            query = query.Where(x => x.DriverId == driverId.Value);
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

        return await Project(query)
            .Skip(Offset(skip)).Take(PageSize(take))
            .ToListAsync(cancellationToken);
    }


    /// <summary>
    /// Driver and transporter names are denormalized into the projection so the history table renders
    /// without per-row lookups.
    /// <para>
    /// The ordering lives INSIDE this query, on entity columns, and must stay here: ordering the
    /// projected <see cref="DriverTransporterAssignmentVm"/> afterwards makes EF order by a member of a
    /// constructor-built record struct, which Npgsql cannot translate — it throws at query time. EF
    /// InMemory happily evaluates it client-side, so unit tests do NOT catch the regression.
    /// </para>
    /// </summary>
    private IQueryable<DriverTransporterAssignmentVm> Project(IQueryable<Entities.DriverTransporterAssignment> query)
        // Most recent assignment first.
        => from assignment in query
           join driver in Context.Drivers on assignment.DriverId equals driver.DriverId
           join transporter in Context.Transporters on assignment.TransporterId equals transporter.TransporterId
           orderby assignment.StartsAt descending, assignment.DriverTransporterAssignmentId
           select new DriverTransporterAssignmentVm(
               assignment.DriverTransporterAssignmentId,
               assignment.AccountId,
               assignment.DriverId,
               driver.Name,
               assignment.TransporterId,
               transporter.Name,
               assignment.StartsAt,
               assignment.EndsAt,
               assignment.AssignmentType,
               assignment.Status,
               assignment.CreatedByPrincipal,
               assignment.LastModified);
}
