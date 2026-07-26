using Common.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class PublicLinkGrantWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IPublicLinkGrantWriter
{
    public async Task<PublicLinkGrantVm> CreatePublicLinkGrantAsync(PublicLinkGrantDto publicLinkGrant, CancellationToken cancellationToken)
    {
        var token = GeneratePublicLinkToken();
        var subjectTokenIdHash = string.IsNullOrWhiteSpace(publicLinkGrant.SubjectTokenIdHash)
            ? PublicLinkTokenHasher.Hash(token)
            : publicLinkGrant.SubjectTokenIdHash;
        var entity = new PublicLinkGrant(RequireAccountWriteAccess(publicLinkGrant.AccountId), publicLinkGrant.ResourceType, publicLinkGrant.ResourceId, publicLinkGrant.Scopes, publicLinkGrant.Purpose, subjectTokenIdHash, publicLinkGrant.ExpiresAt, publicLinkGrant.CreatedByPrincipalId);
        await Context.PublicLinkGrants.AddAsync(entity, cancellationToken);
        AddAuditEvent(entity.AccountId, "CreatePublicLinkGrant", "PublicLinkGrant", entity.PublicLinkGrantId.ToString(), null, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity, string.IsNullOrWhiteSpace(publicLinkGrant.SubjectTokenIdHash) ? token : null);
    }

    public async Task RevokePublicLinkGrantAsync(Guid publicLinkGrantId, string revokedBy, CancellationToken cancellationToken)
    {
        var entity = await Context.PublicLinkGrants.FirstAsync(x => x.PublicLinkGrantId == publicLinkGrantId, cancellationToken);
        RequireAccountWriteAccess(entity.AccountId);
        Context.PublicLinkGrants.Attach(entity);
        var oldValues = AuditValues(entity);
        entity.RevokedAt = DateTimeOffset.UtcNow;
        entity.RevokedBy = revokedBy;
        AddAuditEvent(entity.AccountId, "RevokePublicLinkGrant", "PublicLinkGrant", entity.PublicLinkGrantId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    // RecordPublicLinkAccessAsync removed — see PublicLinkGrantResolver, which is now the only place
    // that increments AccessCount, stamps LastAccessedAt and writes the `PublicLinkAccessed` audit
    // event (spec 11 §7.8/§18.10).

    private static PublicLinkGrantVm ToVm(PublicLinkGrant x, string? token = null) 
        => new(x.PublicLinkGrantId, x.AccountId, x.ResourceType, x.ResourceId, x.Scopes, x.Purpose, x.ExpiresAt, x.RevokedAt, x.RevokedBy, x.CreatedByPrincipalId, x.AccessCount, x.LastAccessedAt, x.LastModified, token);

    private static string GeneratePublicLinkToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string AuditValues(PublicLinkGrant grant)
        => $$"""{"resourceType":"{{grant.ResourceType}}","resourceId":"{{grant.ResourceId}}","scopes":"{{grant.Scopes}}","purpose":"{{grant.Purpose}}","expiresAt":{{Quote(grant.ExpiresAt)}},"revokedAt":{{Quote(grant.RevokedAt)}},"revokedBy":{{Quote(grant.RevokedBy)}},"accessCount":{{grant.AccessCount}},"lastAccessedAt":{{Quote(grant.LastAccessedAt)}}}""";

}
