using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

/// <summary>
/// A platform-wide, administrator-authored banner message. The text is user content (author-written
/// per language), not a system string, so storing it here is consistent with the localization policy
/// — the same precedent as account-authored notification template overrides.
/// </summary>
public sealed class PlatformAnnouncement(string messageEn, string? messageEs, int severity, DateTimeOffset? startsAt, DateTimeOffset? endsAt, bool active) : BaseAuditableEntity
{
    public Guid PlatformAnnouncementId { get; private set; } = Guid.NewGuid();
    public string MessageEn { get; set; } = messageEn;
    /// <summary>Optional — Spanish viewers fall back to <see cref="MessageEn"/> when absent.</summary>
    public string? MessageEs { get; set; } = messageEs;
    public int Severity { get; set; } = severity;
    /// <summary>Null = visible immediately.</summary>
    public DateTimeOffset? StartsAt { get; set; } = startsAt;
    /// <summary>Null = visible until deactivated.</summary>
    public DateTimeOffset? EndsAt { get; set; } = endsAt;
    public bool Active { get; set; } = active;
}
