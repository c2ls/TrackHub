namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsAssignmentHistoryRowVm(
    Guid TransporterId,
    Guid DeviceId,
    string AssignmentType,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    string? Reason);
