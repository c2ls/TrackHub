using Common.Domain.Time;
using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class StaffQualificationReader(IApplicationDbContext context, ICurrentPrincipal principal, IAccountTimeZoneResolver? zones = null) : AccountScopedDataAccess(context, principal), IStaffQualificationReader
{
    private readonly IAccountTimeZoneResolver _zones = zones ?? UtcAccountTimeZoneResolver.Instance;
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<StaffQualificationsPageVm> GetStaffQualificationsAsync(Guid accountId, Guid? staffId, int? expiringWithinDays, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        var query = Context.StaffQualifications.Where(x => x.AccountId == scopedAccountId);

        if (staffId.HasValue)
        {
            query = query.Where(x => x.StaffId == staffId.Value);
        }

        if (expiringWithinDays.HasValue)
        {
            // Past-due rows always qualify — the window is an upper bound, not a band.
            var cutoff = (await _zones.ResolveAsync(scopedAccountId, cancellationToken)).Today().AddDays(Math.Max(0, expiringWithinDays.Value));
            query = query.Where(x => x.ExpiresAt != null && x.ExpiresAt <= cutoff);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await Project(query)
            .Skip(Offset(skip)).Take(PageSize(take))
            .ToListAsync(cancellationToken);
        return new StaffQualificationsPageVm(items, totalCount);
    }

    /// <summary>
    /// StaffName is denormalized into the projection so the account-wide expirations view renders
    /// without a per-row staff lookup.
    /// <para>
    /// The ordering lives INSIDE this query, on entity columns, and must stay here: ordering the
    /// projected <see cref="StaffQualificationVm"/> afterwards makes EF order by a member of a
    /// constructor-built record struct, which Npgsql cannot translate — it throws at query time. EF
    /// InMemory happily evaluates it client-side, so unit tests do NOT catch the regression.
    /// </para>
    /// </summary>
    private IQueryable<StaffQualificationVm> Project(IQueryable<Entities.StaffQualification> query)
        // Soonest expiry first; undated qualifications sink to the bottom.
        => from qualification in query
           join staff in Context.StaffMembers on qualification.StaffId equals staff.StaffId
           orderby qualification.ExpiresAt == null, qualification.ExpiresAt, qualification.StaffQualificationId
           select new StaffQualificationVm(
               qualification.StaffQualificationId,
               qualification.AccountId,
               qualification.StaffId,
               staff.Name,
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
