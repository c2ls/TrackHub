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

        return await context.TransporterDeviceAssignments
            .Where(a => a.Status == (int)AssignmentStatus.Active
                && a.Device.OperatorId == operatorId
                && visibleTransporterIds.Contains(a.TransporterId))
            .Select(a => a.Transporter.Position)
            .Where(tp => tp != null)
            .Distinct()
            .Select(tp => new TransporterPositionVm(
                tp!.TransporterPositionId,
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
        => await context.TransporterDeviceAssignments
            .Where(a => a.Status == (int)AssignmentStatus.Active && a.Device.OperatorId == operatorId)
            .Select(a => a.Transporter.Position)
            .Where(tp => tp != null)
            .Distinct()
            .Select(tp => new TransporterPositionVm(
                tp!.TransporterPositionId,
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
