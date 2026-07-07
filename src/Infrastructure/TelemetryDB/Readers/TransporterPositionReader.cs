using TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;
using TransporterType = Common.Domain.Enums.TransporterType;

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB.Readers;

public sealed class TransporterPositionReader(IApplicationDbContext context, IVisibleTransporterReader visibleReader) : ITransporterPositionReader
{
    public async Task<IReadOnlyCollection<TransporterPositionVm>> GetTransporterPositionsAsync(Guid userId, Guid operatorId, CancellationToken cancellationToken)
    {
        // Stored-projection read for the live map, reimplemented on the single visibility primitive
        // (spec 01.3 A1.2/A1.3): privileged roles read account-wide, plain users are group-scoped.
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
                new(DateTime.SpecifyKind(tp.DateTime, DateTimeKind.Utc), tp.Offset),
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
                new(DateTime.SpecifyKind(tp.DateTime, DateTimeKind.Utc), tp.Offset),
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
