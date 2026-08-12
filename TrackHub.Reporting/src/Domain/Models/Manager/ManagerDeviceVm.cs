namespace TrackHub.Reporting.Domain.Models.Manager;

public readonly record struct ManagerDeviceVm(
    Guid DeviceId,
    Guid AccountId,
    Guid OperatorId,
    string Serial,
    string Name,
    int Identifier,
    string? ProviderDisplayName,
    string? ProviderStatus,
    string DetectedStatus,
    DateTimeOffset FirstSeenAt,
    DateTimeOffset LastSeenAt,
    DateTimeOffset? LastAssignedAt,
    DateTimeOffset? IgnoredAt);
