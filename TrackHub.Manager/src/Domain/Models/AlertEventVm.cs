namespace TrackHub.Manager.Domain.Models;

public readonly record struct AlertEventVm(Guid AlertEventId, Guid AccountId, string EventType, string Severity, string SourceModule, string ResourceType, string ResourceId, string Status, DateTimeOffset FirstSeenAt, DateTimeOffset LastSeenAt, string? PayloadJson, string DeduplicationKey, DateTimeOffset LastModified);
