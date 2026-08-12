namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsRecentlyAddedDeviceRowVm(
    Guid DeviceId,
    int Identifier,
    string Serial,
    string Operator,
    string? ProviderDisplayName,
    DateTimeOffset FirstSeenAt);
