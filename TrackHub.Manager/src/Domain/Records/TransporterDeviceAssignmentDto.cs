namespace TrackHub.Manager.Domain.Records;

public readonly record struct TransporterDeviceAssignmentDto(
    Guid AccountId,
    Guid TransporterId,
    Guid DeviceId,
    int Priority,
    bool IsPrimary,
    string? AssignmentReason);
