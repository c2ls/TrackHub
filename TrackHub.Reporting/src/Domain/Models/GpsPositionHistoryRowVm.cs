namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsPositionHistoryRowVm(
    Guid TransporterId,
    DateTimeOffset DeviceDateTime,
    double Latitude,
    double Longitude,
    Guid SourceDeviceId,
    Guid AccountId);
