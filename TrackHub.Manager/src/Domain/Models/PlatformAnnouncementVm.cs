namespace TrackHub.Manager.Domain.Models;

public readonly record struct PlatformAnnouncementVm(
    Guid PlatformAnnouncementId,
    string MessageEn,
    string? MessageEs,
    AnnouncementSeverity Severity,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    bool Active,
    DateTimeOffset LastModified);
