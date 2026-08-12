using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class AuditEvent(Guid accountId, string actorType, string actorId, string action, string resourceType, string resourceId, string result, string? oldValuesJson, string? newValuesJson, string? reason, string? ipAddress, string? userAgent, string? correlationId) : BaseEntity
{
    public Guid AuditEventId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public string ActorType { get; set; } = actorType;
    public string ActorId { get; set; } = actorId;
    public string Action { get; set; } = action;
    public string ResourceType { get; set; } = resourceType;
    public string ResourceId { get; set; } = resourceId;
    public string Result { get; set; } = result;
    public string? OldValuesJson { get; set; } = oldValuesJson;
    public string? NewValuesJson { get; set; } = newValuesJson;
    public string? Reason { get; set; } = reason;
    public string? IpAddress { get; set; } = ipAddress;
    public string? UserAgent { get; set; } = userAgent;
    public string? CorrelationId { get; set; } = correlationId;
    public DateTimeOffset OccurredAt { get; private set; } = DateTimeOffset.UtcNow;
}
