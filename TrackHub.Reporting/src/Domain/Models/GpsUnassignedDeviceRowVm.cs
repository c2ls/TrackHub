namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsUnassignedDeviceRowVm(
    Guid DeviceId,
    int Identifier,
    string Serial,
    string Operator,
    string? ProviderStatus,
    DateTimeOffset FirstSeenAt,
    DateTimeOffset LastSeenAt);
