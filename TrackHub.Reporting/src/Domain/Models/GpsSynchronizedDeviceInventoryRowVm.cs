namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsSynchronizedDeviceInventoryRowVm(
    Guid DeviceId,
    int Identifier,
    string Serial,
    string Operator,
    string? ProviderStatus,
    string DetectedStatus,
    DateTimeOffset FirstSeenAt,
    DateTimeOffset LastSeenAt,
    string? AssignedTransporter,
    DateTimeOffset? AssignedAt);
