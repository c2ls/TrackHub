namespace TrackHub.Manager.Domain.Models;

public readonly record struct AuditEventVm(Guid AuditEventId, Guid AccountId, string ActorType, string ActorId, string Action, string ResourceType, string ResourceId, string Result, string? Reason, string? IpAddress, string? UserAgent, string? CorrelationId, DateTimeOffset OccurredAt);
