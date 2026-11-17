namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsAssignmentHistoryRowVm(
    string TransporterName,
    string Serial,
    [property: ReportValues("DeviceAssignmentType")] string AssignmentType,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    string? Reason);
