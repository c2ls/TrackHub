using TrackHub.Manager.Domain.Enums;

namespace TrackHub.Manager.Domain.Models;

public readonly record struct TransporterDeviceAssignmentVm(
    Guid TransporterDeviceAssignmentId,
    Guid AccountId,
    Guid TransporterId,
    Guid DeviceId,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    int Priority,
    bool IsPrimary,
    AssignmentStatus Status,
    string? AssignmentReason,
    string CreatedByPrincipalType,
    string CreatedByPrincipalId);
