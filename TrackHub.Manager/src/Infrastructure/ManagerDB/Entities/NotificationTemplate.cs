using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class NotificationTemplate(Guid? accountId, string templateKey, string channel, string locale, string? subject, string body, bool active) : BaseAuditableEntity
{
    public Guid NotificationTemplateId { get; private set; } = Guid.NewGuid();
    /// <summary>Null = platform default template (read-only to accounts).</summary>
    public Guid? AccountId { get; set; } = accountId;
    public string TemplateKey { get; set; } = templateKey;
    public string Channel { get; set; } = channel;
    public string Locale { get; set; } = locale;
    public string? Subject { get; set; } = subject;
    public string Body { get; set; } = body;
    public bool Active { get; set; } = active;
}
