using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class DocumentReader(IApplicationDbContext context, ICurrentPrincipal principal, IDocumentAccessPolicy accessPolicy)
    : AccountScopedDataAccess(context, principal), IDocumentReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    // Confidential/Legal are visible only to a cleared principal.
    private bool CanSeeSensitive => accessPolicy.IsClearedForClassification(DocumentClassifications.Confidential);

    public async Task<IReadOnlyCollection<DocumentVm>> GetDocumentsForOwnerAsync(Guid accountId, string ownerEntityType, string ownerEntityId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);

        // Owner group/assignment visibility — a change from the previous tenant-only behavior.
        if (!await accessPolicy.CanAccessOwnerAsync(scopedAccountId, ownerEntityType, ownerEntityId, forWrite: false, cancellationToken))
        {
            throw new ForbiddenAccessException("Insufficient permissions to read documents for this owner.");
        }

        var canSeeSensitive = CanSeeSensitive;
        var entities = await Context.Documents
            .Where(x => x.AccountId == scopedAccountId
                && x.OwnerEntityType == ownerEntityType
                && x.OwnerEntityId == ownerEntityId
                && x.Status != DocumentStatuses.Deleted
                && (canSeeSensitive || (x.Classification != DocumentClassifications.Confidential && x.Classification != DocumentClassifications.Legal))
                && (!from.HasValue || x.LastModified >= from)
                && (!to.HasValue || x.LastModified <= to))
            .OrderByDescending(x => x.LastModified).ThenBy(x => x.DocumentId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .ToListAsync(cancellationToken);

        return entities.Select(ToVm).ToList();
    }

    public async Task<DocumentVm> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var entity = await LoadAuthorizedDocumentAsync(documentId, cancellationToken);
        return ToVm(entity);
    }

    // Batched compliance projection: every group-visible transporter with its Active document
    // categories, in two queries total. Visibility and sensitive-classification rules match
    // GetDocumentsForOwnerAsync (an uncleared caller does not see Confidential/Legal documents
    // as present).
    public async Task<IReadOnlyCollection<TransporterDocumentComplianceVm>> GetTransporterDocumentComplianceAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        var visibleIds = await accessPolicy.GetVisibleTransporterIdsAsync(scopedAccountId, cancellationToken);

        var transporters = await Context.Transporters
            .Where(t => t.AccountId == scopedAccountId && visibleIds.Contains(t.TransporterId))
            .Select(t => new { t.TransporterId, t.Name })
            .ToListAsync(cancellationToken);

        var ownerIds = transporters.Select(t => t.TransporterId.ToString()).ToList();
        var canSeeSensitive = CanSeeSensitive;
        var activeDocuments = await Context.Documents
            .Where(x => x.AccountId == scopedAccountId
                && x.OwnerEntityType == DocumentOwnerTypes.Transporter
                && ownerIds.Contains(x.OwnerEntityId)
                && x.Status == DocumentStatuses.Active
                && (canSeeSensitive || (x.Classification != DocumentClassifications.Confidential && x.Classification != DocumentClassifications.Legal)))
            .Select(x => new { x.OwnerEntityId, x.Category })
            .ToListAsync(cancellationToken);

        var categoriesByOwner = activeDocuments
            .GroupBy(d => d.OwnerEntityId)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<string>)g.Select(d => d.Category).Distinct(StringComparer.OrdinalIgnoreCase).ToList());

        return transporters
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .Select(t => new TransporterDocumentComplianceVm(
                t.TransporterId,
                t.Name,
                categoriesByOwner.GetValueOrDefault(t.TransporterId.ToString(), [])))
            .ToList();
    }

    public async Task<IReadOnlyCollection<DocumentVersionVm>> GetDocumentVersionsAsync(Guid documentId, int skip, int take, CancellationToken cancellationToken)
    {
        await LoadAuthorizedDocumentAsync(documentId, cancellationToken);

        return await Context.DocumentVersions
            .Where(v => v.DocumentId == documentId)
            // total-order: (DocumentId, VersionNumber) is a unique index and DocumentId is fixed by
            // the filter above, so VersionNumber alone already totally orders this result and no
            // tiebreaker is possible.
            .OrderByDescending(v => v.VersionNumber)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(v => new DocumentVersionVm(v.DocumentVersionId, v.DocumentId, v.AccountId, v.VersionNumber, v.ContentType, v.FileName, v.SizeBytes, v.Sha256Hash, v.ScanStatus, v.ReplacedByPrincipalType, v.ReplacedByPrincipalId, v.Reason, v.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentVm?> GetActiveDocumentByCategoryAsync(string ownerEntityType, string ownerEntityId, string category, CancellationToken cancellationToken)
    {
        var accountId = ResolveAccountScope(null)
            ?? throw new ForbiddenAccessException("Current principal must resolve an account id.");

        if (!await accessPolicy.CanAccessOwnerAsync(accountId, ownerEntityType, ownerEntityId, forWrite: false, cancellationToken))
        {
            throw new ForbiddenAccessException("Insufficient permissions to read documents for this owner.");
        }

        var entity = await Context.Documents
            .Where(x => x.AccountId == accountId
                && x.OwnerEntityType == ownerEntityType
                && x.OwnerEntityId == ownerEntityId
                && x.Category == category
                && x.Status == DocumentStatuses.Active)
            .OrderByDescending(x => x.LastModified)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null || (!CanSeeSensitive && DocumentClassifications.IsSensitive(entity.Classification)))
        {
            return null;
        }

        return ToVm(entity);
    }

    public async Task<IReadOnlyCollection<DocumentVm>> SearchDocumentsAsync(DocumentSearchFilter filter, int skip, int take, CancellationToken cancellationToken)
    {
        var accountId = ResolveAccountScope(null)
            ?? throw new ForbiddenAccessException("Current principal must resolve an account id.");

        var query = Context.Documents.Where(x => x.AccountId == accountId && x.Status != DocumentStatuses.Deleted);

        // Group scoping for non-admins: restrict to visible Transporter owners.
        if (!accessPolicy.IsPrivilegedPrincipal)
        {
            var visible = (await accessPolicy.GetVisibleTransporterIdsAsync(accountId, cancellationToken))
                .Select(id => id.ToString()).ToHashSet();
            query = query.Where(x => x.OwnerEntityType == DocumentOwnerTypes.Transporter && visible.Contains(x.OwnerEntityId));
        }

        if (!CanSeeSensitive)
        {
            query = query.Where(x => x.Classification != DocumentClassifications.Confidential && x.Classification != DocumentClassifications.Legal);
        }

        if (!string.IsNullOrWhiteSpace(filter.OwnerEntityType))
        {
            query = query.Where(x => x.OwnerEntityType == filter.OwnerEntityType);
        }
        if (!string.IsNullOrWhiteSpace(filter.OwnerEntityId))
        {
            query = query.Where(x => x.OwnerEntityId == filter.OwnerEntityId);
        }
        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(x => x.Category == filter.Category);
        }
        if (!string.IsNullOrWhiteSpace(filter.Classification))
        {
            query = query.Where(x => x.Classification == filter.Classification);
        }
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(x => x.Status == filter.Status);
        }
        if (!string.IsNullOrWhiteSpace(filter.UploadedByPrincipalId))
        {
            query = query.Where(x => x.UploadedByPrincipalId == filter.UploadedByPrincipalId);
        }
        if (filter.From.HasValue)
        {
            query = query.Where(x => x.LastModified >= filter.From);
        }
        if (filter.To.HasValue)
        {
            query = query.Where(x => x.LastModified <= filter.To);
        }
        if (filter.ExpiringWithinDays.HasValue)
        {
            var threshold = DateTimeOffset.UtcNow.AddDays(filter.ExpiringWithinDays.Value);
            query = query.Where(x => x.Status == DocumentStatuses.Active && x.ExpiresAt != null && x.ExpiresAt <= threshold);
        }

        var entities = await query
            .OrderByDescending(x => x.LastModified).ThenBy(x => x.DocumentId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .ToListAsync(cancellationToken);

        return entities.Select(ToVm).ToList();
    }

    public async Task<IReadOnlyCollection<DocumentVm>> GetExpiringDocumentsAsync(int withinDays, int skip, int take, CancellationToken cancellationToken)
    {
        var accountId = ResolveAccountScope(null)
            ?? throw new ForbiddenAccessException("Current principal must resolve an account id.");

        var threshold = DateTimeOffset.UtcNow.AddDays(Math.Max(0, withinDays));
        var query = Context.Documents.Where(x => x.AccountId == accountId
            && x.Status == DocumentStatuses.Active
            && x.ExpiresAt != null
            && x.ExpiresAt <= threshold);

        if (!accessPolicy.IsPrivilegedPrincipal)
        {
            var visible = (await accessPolicy.GetVisibleTransporterIdsAsync(accountId, cancellationToken))
                .Select(id => id.ToString()).ToHashSet();
            query = query.Where(x => x.OwnerEntityType == DocumentOwnerTypes.Transporter && visible.Contains(x.OwnerEntityId));
        }

        if (!CanSeeSensitive)
        {
            query = query.Where(x => x.Classification != DocumentClassifications.Confidential && x.Classification != DocumentClassifications.Legal);
        }

        var entities = await query
            .OrderBy(x => x.ExpiresAt).ThenBy(x => x.DocumentId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .ToListAsync(cancellationToken);

        return entities.Select(ToVm).ToList();
    }

    public async Task<IReadOnlyCollection<PublicLinkGrantVm>> GetDocumentSharesAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await LoadAuthorizedDocumentAsync(documentId, cancellationToken);
        var resourceId = documentId.ToString();

        return await Context.PublicLinkGrants
            .Where(g => g.AccountId == document.AccountId && g.ResourceType == DocumentSharing.ResourceType && g.ResourceId == resourceId)
            .OrderByDescending(g => g.LastModified)
            .Select(g => new PublicLinkGrantVm(g.PublicLinkGrantId, g.AccountId, g.ResourceType, g.ResourceId, g.Scopes, g.Purpose, g.ExpiresAt, g.RevokedAt, g.RevokedBy, g.CreatedByPrincipalId, g.AccessCount, g.LastAccessedAt, g.LastModified, null))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DocumentTypeVm>> GetDocumentTypesAsync(Guid accountId, bool includeDisabled, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);

        return await Context.DocumentTypes
            .Where(t => t.AccountId == scopedAccountId && (includeDisabled || t.Enabled))
            .OrderBy(t => t.Category)
            .Select(t => new DocumentTypeVm(t.DocumentTypeId, t.AccountId, t.Category, t.DisplayName, t.Required, t.Expiring, t.DefaultValidityDays, t.Enabled, t.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DocumentSignatureVm>> GetDocumentSignaturesAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await LoadAuthorizedDocumentAsync(documentId, cancellationToken);

        return await Context.DocumentSignatures
            .Where(s => s.DocumentId == documentId && s.AccountId == document.AccountId)
            .OrderByDescending(s => s.SignedAt)
            .Select(s => new DocumentSignatureVm(s.DocumentSignatureId, s.DocumentId, s.AccountId, s.SignerPrincipalType, s.SignerPrincipalId, s.SignerName, s.SignatureImageDocumentId, s.LegalTextAccepted, s.Latitude, s.Longitude, s.SignedAt, s.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    // Loads a document and enforces the full read authorization chain: account scope, owner visibility,
    // then classification clearance. Non-disclosable failures surface as 404.
    private async Task<Document> LoadAuthorizedDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var entity = await Context.Documents.FirstOrDefaultAsync(x => x.DocumentId == documentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Document), documentId.ToString());

        RequireAccountAccess(entity.AccountId);

        if (!await accessPolicy.CanAccessOwnerAsync(entity.AccountId, entity.OwnerEntityType, entity.OwnerEntityId, forWrite: false, cancellationToken))
        {
            throw new NotFoundException(nameof(Document), documentId.ToString());
        }

        // Non-disclosure: an uncleared sensitive document is hidden exactly like the listing paths
        // (filtered out / null), so a direct read must 404 rather than confirm existence via 403.
        if (!accessPolicy.IsClearedForClassification(entity.Classification))
        {
            throw new NotFoundException(nameof(Document), documentId.ToString());
        }

        return entity;
    }

    private DocumentVm ToVm(Document x)
    {
        // DownloadUrl is populated only after authorization + ScanStatus == Clean.
        var downloadUrl = string.Equals(x.ScanStatus, DocumentScanStatuses.Clean, StringComparison.OrdinalIgnoreCase)
            ? $"/documents/{x.DocumentId}/download"
            : null;

        return new DocumentVm(x.DocumentId, x.AccountId, x.OwnerEntityType, x.OwnerEntityId, x.UploadedByPrincipalType, x.UploadedByPrincipalId, x.FileName, x.Category, x.Title, x.Description, x.ContentType, x.SizeBytes, x.Sha256Hash, x.Classification, x.Status, x.ExpiresAt, x.VisibilityScope, x.ScanStatus, x.CurrentVersion, downloadUrl, x.LastModified);
    }
}
