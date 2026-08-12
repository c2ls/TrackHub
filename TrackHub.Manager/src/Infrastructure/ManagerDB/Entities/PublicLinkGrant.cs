using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class PublicLinkGrant(Guid accountId, string resourceType, string resourceId, string scopes, string purpose, string subjectTokenIdHash, DateTimeOffset expiresAt, string createdBy) : BaseAuditableEntity
{
    public Guid PublicLinkGrantId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public string ResourceType { get; set; } = resourceType;
    public string ResourceId { get; set; } = resourceId;
    public string Scopes { get; set; } = scopes;
    public string Purpose { get; set; } = purpose;
    public string SubjectTokenIdHash { get; set; } = subjectTokenIdHash;
    public DateTimeOffset ExpiresAt { get; set; } = expiresAt;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
    public string CreatedByPrincipalId { get; set; } = createdBy;
    public int AccessCount { get; set; }
    public DateTimeOffset? LastAccessedAt { get; set; }
}
