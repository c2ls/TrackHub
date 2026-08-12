using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class AlertEvent(Guid accountId, string eventType, string severity, string sourceModule, string resourceType, string resourceId, string status, string? payloadJson, string deduplicationKey) : BaseAuditableEntity
{
    public Guid AlertEventId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public string EventType { get; set; } = eventType;
    public string Severity { get; set; } = severity;
    public string SourceModule { get; set; } = sourceModule;
    public string ResourceType { get; set; } = resourceType;
    public string ResourceId { get; set; } = resourceId;
    public string Status { get; set; } = status;
    public DateTimeOffset FirstSeenAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastSeenAt { get; set; } = DateTimeOffset.UtcNow;
    public string? PayloadJson { get; set; } = payloadJson;
    public string DeduplicationKey { get; set; } = deduplicationKey;
}
