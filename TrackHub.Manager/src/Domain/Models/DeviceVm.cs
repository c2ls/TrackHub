namespace TrackHub.Manager.Domain.Models;

public readonly record struct DeviceVm(
    Guid DeviceId,
    Guid AccountId,
    Guid OperatorId,
    string Serial,
    string Name,
    int Identifier,
    string? ProviderDisplayName,
    DeviceType DeviceType,
    short DeviceTypeId,
    string? Description,
    string? ProviderMetadataHash,
    string? ProviderStatus,
    DetectedStatus DetectedStatus,
    DateTimeOffset FirstSeenAt,
    DateTimeOffset LastSeenAt,
    DateTimeOffset LastSyncedAt,
    DateTimeOffset? LastAssignedAt,
    DateTimeOffset? IgnoredAt);
