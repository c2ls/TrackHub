using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class DriverQualificationReader(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IDriverQualificationReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<IReadOnlyCollection<DriverQualificationVm>> GetDriverQualificationsAsync(Guid accountId, Guid? driverId, int? expiringWithinDays, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        var query = Context.DriverQualifications.Where(x => x.AccountId == scopedAccountId);

        if (driverId.HasValue)
        {
            query = query.Where(x => x.DriverId == driverId.Value);
        }

        if (expiringWithinDays.HasValue)
        {
            // Past-due rows always qualify — the window is an upper bound, not a band.
            var cutoff = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime).AddDays(Math.Max(0, expiringWithinDays.Value));
            query = query.Where(x => x.ExpiresAt != null && x.ExpiresAt <= cutoff);
        }

        return await Project(query)
            .Skip(Offset(skip)).Take(PageSize(take))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// DriverName is denormalized into the projection so the account-wide expirations view renders
    /// without a per-row driver lookup.
    /// <para>
    /// The ordering lives INSIDE this query, on entity columns, and must stay here: ordering the
    /// projected <see cref="DriverQualificationVm"/> afterwards makes EF order by a member of a
    /// constructor-built record struct, which Npgsql cannot translate — it throws at query time. EF
    /// InMemory happily evaluates it client-side, so unit tests do NOT catch the regression.
    /// </para>
    /// </summary>
    private IQueryable<DriverQualificationVm> Project(IQueryable<Entities.DriverQualification> query)
        // Soonest expiry first; undated qualifications sink to the bottom.
        => from qualification in query
           join driver in Context.Drivers on qualification.DriverId equals driver.DriverId
           orderby qualification.ExpiresAt == null, qualification.ExpiresAt, qualification.DriverQualificationId
           select new DriverQualificationVm(
               qualification.DriverQualificationId,
               qualification.AccountId,
               qualification.DriverId,
               driver.Name,
               qualification.QualificationType,
               qualification.Category,
               qualification.Number,
               qualification.IssuedAt,
               qualification.ExpiresAt,
               qualification.IssuingAuthority,
               qualification.Status,
               qualification.DocumentId,
               qualification.Notes,
               qualification.LastModified);
}
