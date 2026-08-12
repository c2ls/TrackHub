namespace TrackHub.Manager.Domain.Records;

public readonly record struct AuditEventDto(Guid AccountId, string ActorType, string ActorId, string Action, string ResourceType, string ResourceId, string Result, string? OldValuesJson, string? NewValuesJson, string? Reason, string? IpAddress, string? UserAgent, string? CorrelationId);
