namespace TrackHub.Reporting.Domain.Models.Manager;

public readonly record struct ManagerTransporterDeviceAssignmentVm(
    Guid TransporterDeviceAssignmentId,
    Guid AccountId,
    Guid TransporterId,
    Guid DeviceId,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    int Priority,
    bool IsPrimary,
    string Status,
    string? AssignmentReason);
