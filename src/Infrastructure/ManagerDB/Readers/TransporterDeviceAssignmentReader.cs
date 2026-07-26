using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Enums;
using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class TransporterDeviceAssignmentReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), ITransporterDeviceAssignmentReader
{
    public async Task<TransporterDeviceAssignmentVm> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Context.TransporterDeviceAssignments
            .Where(a => a.TransporterDeviceAssignmentId == id)
            .Select(a => new { a.AccountId, Vm = Project(a) })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(TrackHub.Manager.Infrastructure.Entities.TransporterDeviceAssignment), $"{id}");
        RequireAccountAccess(entity.AccountId);
        return entity.Vm;
    }

    public async Task<TransporterDeviceAssignmentsPageVm> GetByTransporterAsync(Guid transporterId, bool activeOnly, int skip, int take, CancellationToken cancellationToken)
    {
        var transporter = await Context.Transporters.Where(t => t.TransporterId == transporterId)
            .Select(t => new { t.AccountId }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Transporter", $"{transporterId}");
        RequireAccountAccess(transporter.AccountId);
        var q = Context.TransporterDeviceAssignments.Where(a => a.TransporterId == transporterId);
        if (activeOnly) q = q.Where(a => a.Status == (int)AssignmentStatus.Active);
        return await PageAsync(q, skip, take, cancellationToken);
    }

    public async Task<TransporterDeviceAssignmentsPageVm> GetByAccountAsync(Guid accountId, bool activeOnly, int skip, int take, CancellationToken cancellationToken)
    {
        var scoped = RequireAccountAccess(accountId);
        var q = Context.TransporterDeviceAssignments.Where(a => a.AccountId == scoped);
        if (activeOnly) q = q.Where(a => a.Status == (int)AssignmentStatus.Active);
        return await PageAsync(q, skip, take, cancellationToken);
    }

    // EffectiveFrom is the meaningful sort but ties are routine — a bulk assignment writes many rows
    // with the same instant — so the primary key completes the total order the page window needs.
    private static async Task<TransporterDeviceAssignmentsPageVm> PageAsync(
        IQueryable<TrackHub.Manager.Infrastructure.Entities.TransporterDeviceAssignment> query,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.EffectiveFrom)
            .ThenBy(a => a.TransporterDeviceAssignmentId)
            .Skip(skip)
            .Take(take)
            .Select(ProjectExpr)
            .ToListAsync(cancellationToken);

        return new TransporterDeviceAssignmentsPageVm(items, totalCount);
    }

    private static System.Linq.Expressions.Expression<Func<TrackHub.Manager.Infrastructure.Entities.TransporterDeviceAssignment, TransporterDeviceAssignmentVm>> ProjectExpr =>
        a => new TransporterDeviceAssignmentVm(a.TransporterDeviceAssignmentId, a.AccountId, a.TransporterId, a.DeviceId,
            a.EffectiveFrom, a.EffectiveTo, a.Priority, a.IsPrimary, (AssignmentStatus)a.Status, a.AssignmentReason,
            a.CreatedByPrincipalType, a.CreatedBy ?? string.Empty);

    private static TransporterDeviceAssignmentVm Project(TrackHub.Manager.Infrastructure.Entities.TransporterDeviceAssignment a) =>
        new(a.TransporterDeviceAssignmentId, a.AccountId, a.TransporterId, a.DeviceId,
            a.EffectiveFrom, a.EffectiveTo, a.Priority, a.IsPrimary, (AssignmentStatus)a.Status, a.AssignmentReason,
            a.CreatedByPrincipalType, a.CreatedBy ?? string.Empty);
}
