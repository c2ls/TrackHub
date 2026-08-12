namespace TrackHub.Manager.Domain.Records;

public readonly record struct PublicLinkGrantDto(Guid AccountId, string ResourceType, string ResourceId, string Scopes, string Purpose, string? SubjectTokenIdHash, DateTimeOffset ExpiresAt, string CreatedByPrincipalId);
