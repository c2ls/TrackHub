namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsIgnoredDeviceRowVm(
    Guid DeviceId,
    int Identifier,
    string Serial,
    string Operator,
    DateTimeOffset? IgnoredAt);
