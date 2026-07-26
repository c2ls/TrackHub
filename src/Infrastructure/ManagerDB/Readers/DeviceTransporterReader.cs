using Common.Domain.Helpers;
using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;
using TransporterType = Common.Domain.Enums.TransporterType;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class DeviceTransporterReader(IApplicationDbContext context, ICurrentPrincipal principal, IVisibleTransporterReader visibleReader)
    : AccountScopedDataAccess(context, principal), IDeviceTransporterReader
{
    public async Task<IReadOnlyCollection<DeviceTransporterVm>> GetDeviceTransporterByUserAsync(Guid userId, Guid operatorId, CancellationToken cancellationToken)
    {
        // Reimplemented on the single visibility primitive: privileged roles
        // read account-wide, plain users are group-scoped. The map, its stored fallback, and the
        // replay check all answer through GetVisibleTransporterIdsAsync so they cannot diverge (K1).
        var accountId = await Context.Users
            .Where(u => u.UserId == userId)
            .Select(u => u.AccountId)
            .FirstOrDefaultAsync(cancellationToken);
        var visibleTransporterIds = (await visibleReader.GetVisibleTransporterIdsAsync(userId, accountId, cancellationToken)).ToArray();

        return await Context.TransporterDeviceAssignments
            .Where(a => a.Status == (int)AssignmentStatus.Active
                && a.Device.OperatorId == operatorId
                && visibleTransporterIds.Contains(a.TransporterId))
            .Select(a => new DeviceTransporterVm(
                a.TransporterId,
                a.Device.Identifier,
                a.Device.Serial,
                a.Transporter.Name,
                (TransporterType)a.Transporter.TransporterTypeId,
                a.Transporter.TransporterTypeId))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DeviceTransporterVm>> GetDeviceTransportersAsync(Filters filters, CancellationToken cancellationToken)
    {
        var query = Context.TransporterDeviceAssignments
            .Where(a => a.Status == (int)AssignmentStatus.Active)
            .AsQueryable();
        query = filters.Apply(query);

        return await query
            .Select(a => new DeviceTransporterVm(
                a.TransporterId,
                a.Device.Identifier,
                a.Device.Serial,
                a.Transporter.Name,
                (TransporterType)a.Transporter.TransporterTypeId,
                a.Transporter.TransporterTypeId))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DeviceTransporterVm>> GetAssignedDeviceTransportersByOperatorAsync(Guid accountId, Guid operatorId, CancellationToken cancellationToken)
    {
        var scoped = RequireAccountAccess(accountId);
        return await Context.TransporterDeviceAssignments
            .Where(a => a.AccountId == scoped
                && a.Status == (int)AssignmentStatus.Active
                && a.Device.AccountId == scoped
                && a.Device.OperatorId == operatorId)
            .Select(a => new DeviceTransporterVm(
                a.TransporterId,
                a.Device.Identifier,
                a.Device.Serial,
                a.Transporter.Name,
                (TransporterType)a.Transporter.TransporterTypeId,
                a.Transporter.TransporterTypeId))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<DeviceTransporterVm> GetDeviceTransporterAsync(Guid transporterId, CancellationToken cancellationToken)
    {
        // Authorize against the transporter's owning account before returning device identifiers.
        var transporterAccountId = await Context.Transporters
            .Where(t => t.TransporterId == transporterId)
            .Select(t => (Guid?)t.AccountId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Entities.Transporter), transporterId.ToString());

        RequireAccountAccess(transporterAccountId);

        return await Context.TransporterDeviceAssignments
            .Where(a => a.TransporterId == transporterId && a.Status == (int)AssignmentStatus.Active)
            .OrderByDescending(a => a.IsPrimary)
            .ThenBy(a => a.Priority)
            .Select(a => new DeviceTransporterVm(
                a.TransporterId,
                a.Device.Identifier,
                a.Device.Serial,
                a.Transporter.Name,
                (TransporterType)a.Transporter.TransporterTypeId,
                a.Transporter.TransporterTypeId))
            .FirstAsync(cancellationToken);
    }
}
