using TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;
using TransporterType = Common.Domain.Enums.TransporterType;

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB.Readers;

public sealed class TransporterPositionReader(IApplicationDbContext context, IVisibleTransporterReader visibleReader) : ITransporterPositionReader
{
    public async Task<IReadOnlyCollection<TransporterPositionVm>> GetTransporterPositionsAsync(Guid userId, Guid operatorId, CancellationToken cancellationToken)
    {
        // Stored-projection read for the live map, reimplemented on the single visibility primitive
        //: privileged roles read account-wide, plain users are group-scoped.
        var accountId = await context.Users
            .Where(u => u.UserId == userId)
            .Select(u => u.AccountId)
            .FirstOrDefaultAsync(cancellationToken);
        var visibleTransporterIds = (await visibleReader.GetVisibleTransporterIdsAsync(userId, accountId, cancellationToken)).ToArray();

        // Query the positions directly with an EXISTS over the active assignments: a transporter
        // with several active devices must yield its single latest-position row once, and a
        // DISTINCT over the entity is not translatable (the json attributes column has no
        // equality operator in PostgreSQL).
        return await context.TransporterPositions
            .Where(tp => visibleTransporterIds.Contains(tp.TransporterId)
                && context.TransporterDeviceAssignments.Any(a =>
                    a.Status == (int)AssignmentStatus.Active
                    && a.TransporterId == tp.TransporterId
                    && a.Device.OperatorId == operatorId))
            .Select(tp => new TransporterPositionVm(
                tp.TransporterPositionId,
                tp.TransporterId,
                tp.Transporter.Name,
                (TransporterType)tp.Transporter.TransporterTypeId,
                tp.GeometryId,
                tp.Latitude,
                tp.Longitude,
                tp.Altitude,
                tp.DeviceDateTime,
                tp.Speed,
                tp.Course,
                tp.EventId,
                tp.Address,
                tp.City,
                tp.State,
                tp.Country,
                tp.Attributes))
            .ToListAsync(cancellationToken);
    }

    // Batched variant of the live-map read: one round trip for ALL of the caller's operators
    // instead of one call per operator. Same visibility rules as the singular overload.
    public async Task<IReadOnlyCollection<TransporterPositionVm>> GetTransporterPositionsAsync(Guid userId, IReadOnlyCollection<Guid> operatorIds, CancellationToken cancellationToken)
    {
        if (operatorIds.Count == 0)
        {
            return [];
        }

        var accountId = await context.Users
            .Where(u => u.UserId == userId)
            .Select(u => u.AccountId)
            .FirstOrDefaultAsync(cancellationToken);
        var visibleTransporterIds = (await visibleReader.GetVisibleTransporterIdsAsync(userId, accountId, cancellationToken)).ToArray();
        var operatorIdArray = operatorIds.ToArray();

        return await context.TransporterPositions
            .Where(tp => visibleTransporterIds.Contains(tp.TransporterId)
                && context.TransporterDeviceAssignments.Any(a =>
                    a.Status == (int)AssignmentStatus.Active
                    && a.TransporterId == tp.TransporterId
                    && operatorIdArray.Contains(a.Device.OperatorId)))
            .Select(tp => new TransporterPositionVm(
                tp.TransporterPositionId,
                tp.TransporterId,
                tp.Transporter.Name,
                (TransporterType)tp.Transporter.TransporterTypeId,
                tp.GeometryId,
                tp.Latitude,
                tp.Longitude,
                tp.Altitude,
                tp.DeviceDateTime,
                tp.Speed,
                tp.Course,
                tp.EventId,
                tp.Address,
                tp.City,
                tp.State,
                tp.Country,
                tp.Attributes))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TransporterPositionVm>> GetTransporterPositionsAsync(Guid operatorId, CancellationToken cancellationToken)
        => await context.TransporterPositions
            .Where(tp => context.TransporterDeviceAssignments.Any(a =>
                a.Status == (int)AssignmentStatus.Active
                && a.TransporterId == tp.TransporterId
                && a.Device.OperatorId == operatorId))
            .Select(tp => new TransporterPositionVm(
                tp.TransporterPositionId,
                tp.TransporterId,
                tp.Transporter.Name,
                (TransporterType)tp.Transporter.TransporterTypeId,
                tp.GeometryId,
                tp.Latitude,
                tp.Longitude,
                tp.Altitude,
                tp.DeviceDateTime,
                tp.Speed,
                tp.Course,
                tp.EventId,
                tp.Address,
                tp.City,
                tp.State,
                tp.Country,
                tp.Attributes))
            .ToListAsync(cancellationToken);
}
