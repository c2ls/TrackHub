namespace TrackHub.Manager.Domain.Records;

public readonly record struct PlatformAnnouncementDto(
    string MessageEn,
    string? MessageEs,
    AnnouncementSeverity Severity,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    bool Active);
