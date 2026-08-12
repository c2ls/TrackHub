namespace TrackHub.Manager.Domain.Records;

public readonly record struct AlertEventDto(Guid AccountId, string EventType, string Severity, string SourceModule, string ResourceType, string ResourceId, string Status, string? PayloadJson, string DeduplicationKey);
