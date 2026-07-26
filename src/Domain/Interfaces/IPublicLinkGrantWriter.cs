namespace TrackHub.Manager.Domain.Interfaces;

public interface IPublicLinkGrantWriter
{
    Task<PublicLinkGrantVm> CreatePublicLinkGrantAsync(PublicLinkGrantDto publicLinkGrant, CancellationToken cancellationToken);
    Task RevokePublicLinkGrantAsync(Guid publicLinkGrantId, string revokedBy, CancellationToken cancellationToken);

    // No RecordPublicLinkAccessAsync here by design — access counting belongs to
    // IPublicLinkGrantResolver, the single resolution path (spec 11 §7.8/§18.10).
}
