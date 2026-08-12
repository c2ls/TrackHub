namespace TrackHub.Manager.Domain.Models;

public readonly record struct PublicLinkGrantVm(Guid PublicLinkGrantId, Guid AccountId, string ResourceType, string ResourceId, string Scopes, string Purpose, DateTimeOffset ExpiresAt, DateTimeOffset? RevokedAt, string? RevokedBy, string CreatedByPrincipalId, int AccessCount, DateTimeOffset? LastAccessedAt, DateTimeOffset LastModified, string? Token);
