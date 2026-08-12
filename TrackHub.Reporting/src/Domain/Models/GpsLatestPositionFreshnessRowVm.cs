namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsLatestPositionFreshnessRowVm(
    Guid TransporterId,
    string TransporterName,
    DateTimeOffset? LastDeviceDateTime,
    double? AgeMinutes,
    string? Source);
