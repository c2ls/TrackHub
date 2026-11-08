using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Domain.Enums;
using Common.Domain.Evidence;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class DocumentWriter(IApplicationDbContext context, ICurrentPrincipal principal, IDocumentAccessPolicy accessPolicy, IAlertRecorder alerts)
    : AccountScopedDataAccess(context, principal), IDocumentWriter
{
    public async Task<DocumentVm> RegisterUploadedDocumentAsync(Guid documentId, DocumentDto document, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(document.AccountId);
        await EnsureOwnerWriteAccessAsync(accountId, document.OwnerEntityType, document.OwnerEntityId, cancellationToken);
        document = await ValidateUploadAsync(accountId, document, cancellationToken);

        var entity = BuildDocument(accountId, document);
        entity.DocumentId = documentId;
        entity.Status = DocumentStatuses.Uploaded;
        entity.ScanStatus = DocumentScanStatuses.Quarantined;
        entity.CapturedLatitude = document.CapturedLatitude;
        entity.CapturedLongitude = document.CapturedLongitude;
        entity.CapturedAtDeviceTime = document.CapturedAtDeviceTime;
        entity.SourceDeviceRegistrationId = document.SourceDeviceRegistrationId;

        await Context.Documents.AddAsync(entity, cancellationToken);
        AddDocumentVersion(entity, document.StorageProvider, document.StorageKey, document.Sha256Hash, document.SizeBytes, document.ContentType, document.FileName, DocumentScanStatuses.Quarantined, reason: null);
        AddAuditEvent(accountId, "UploadDocument", "Document", entity.DocumentId.ToString(), null, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task MarkDocumentUploadedAsync(Guid documentId, string status, CancellationToken cancellationToken)
        => await UpdateDocumentAsync(documentId, "MarkDocumentUploaded", x => x.Status = status, cancellationToken);

    public async Task MarkDocumentScanResultAsync(Guid documentId, string scanStatus, CancellationToken cancellationToken)
    {
        var entity = await Context.Documents
            .AsTracking().FirstOrDefaultAsync(x => x.DocumentId == documentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Document), documentId.ToString());
        RequireRowAccess(entity.AccountId, nameof(Document), documentId.ToString(), forWrite: true);
        var oldValues = AuditValues(entity);

        entity.ScanStatus = scanStatus;
        // Servable bytes graduate a still-quarantined document to Active.
        if (DocumentScanStatuses.IsServable(scanStatus) && entity.Status == DocumentStatuses.Uploaded)
        {
            entity.Status = DocumentStatuses.Active;
        }

        // The verdict belongs to the BYTES, and the bytes are the current version's. Writing both
        // keeps the document-level column a safe denormalisation of the version it describes.
        var version = await Context.DocumentVersions
            .AsTracking()
            .FirstOrDefaultAsync(v => v.DocumentId == entity.DocumentId && v.VersionNumber == entity.CurrentVersion, cancellationToken);
        if (version is not null)
        {
            version.ScanStatus = scanStatus;
        }

        AddAuditEvent(entity.AccountId, "MarkDocumentScanResult", "Document", entity.DocumentId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExpireDocumentAsync(Guid documentId, DateTimeOffset expiresAt, CancellationToken cancellationToken)
        => await UpdateDocumentAsync(documentId, "ExpireDocument", x => { x.ExpiresAt = expiresAt; x.Status = DocumentStatuses.Expired; }, cancellationToken);

    public async Task DeleteDocumentReferenceAsync(Guid documentId, CancellationToken cancellationToken)
        => await UpdateDocumentAsync(documentId, "DeleteDocumentReference", x => x.Status = DocumentStatuses.Deleted, cancellationToken, revokeShares: true);

    public async Task<DocumentVm> ReplaceDocumentVersionAsync(Guid documentId, DocumentVersionDto newVersion, CancellationToken cancellationToken)
    {
        var entity = await Context.Documents
            .AsTracking().FirstOrDefaultAsync(x => x.DocumentId == documentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Document), documentId.ToString());
        RequireRowAccess(entity.AccountId, nameof(Document), documentId.ToString(), forWrite: true);
        await EnsureOwnerWriteAccessAsync(entity.AccountId, entity.OwnerEntityType, entity.OwnerEntityId, cancellationToken);

        var oldValues = AuditValues(entity);
        var nextVersion = entity.CurrentVersion + 1;

        var version = new DocumentVersion(entity.DocumentId, entity.AccountId, nextVersion, newVersion.StorageProvider, newVersion.StorageKey, newVersion.Sha256Hash, newVersion.SizeBytes, newVersion.ContentType, newVersion.FileName, DocumentScanStatuses.Quarantined, Principal.PrincipalType.ToString(), ActorId(), newVersion.Reason, DateTimeOffset.UtcNow);
        await Context.DocumentVersions.AddAsync(version, cancellationToken);

        // Re-point the active version; new bytes must be re-scanned before download.
        entity.CurrentVersion = nextVersion;
        entity.StorageProvider = newVersion.StorageProvider;
        entity.StorageKey = newVersion.StorageKey;
        entity.Sha256Hash = newVersion.Sha256Hash;
        entity.SizeBytes = newVersion.SizeBytes;
        entity.ContentType = newVersion.ContentType;
        entity.FileName = newVersion.FileName;
        var renewed = newVersion.ExpiresAt is { } expiresAt && expiresAt != entity.ExpiresAt;
        if (renewed)
        {
            entity.ExpiresAt = newVersion.ExpiresAt;
        }

        // An expired document stays expired until a version brings a future expiry with it.
        entity.Status = entity.Status == DocumentStatuses.Expired && !(entity.ExpiresAt > DateTimeOffset.UtcNow)
            ? DocumentStatuses.Expired
            : DocumentStatuses.Active;
        entity.ScanStatus = DocumentScanStatuses.Quarantined;

        AddAuditEvent(entity.AccountId, "ReplaceDocumentVersion", "Document", entity.DocumentId.ToString(), oldValues, AuditValues(entity));
        try
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (Common.Infrastructure.UniqueViolation.Matches(exception, "IX_document_versions_documentid_versionnumber"))
        {
            // Another upload took this version number first; nothing of this one was written.
            Common.Infrastructure.UniqueViolation.Detach(Context.ChangeTracker);
            throw new ConflictException("Another version of this document was uploaded at the same time. Reload it and try again.");
        }

        if (renewed)
        {
            await alerts.ResolveOpenAsync(entity.AccountId, "Document", entity.DocumentId.ToString(),
                [AlertEventTypes.DocumentExpiring, AlertEventTypes.DocumentExpired], cancellationToken);
        }
        return ToVm(entity);
    }

    public async Task VoidDocumentAsync(Guid documentId, string reason, CancellationToken cancellationToken)
        => await UpdateDocumentAsync(documentId, "VoidDocument", x => x.Status = DocumentStatuses.Voided, cancellationToken, reason, revokeShares: true);

    public async Task<DocumentSignatureVm> SignDocumentAsync(DocumentSignatureDto signature, CancellationToken cancellationToken)
    {
        var document = await Context.Documents.FirstOrDefaultAsync(x => x.DocumentId == signature.DocumentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Document), signature.DocumentId.ToString());
        RequireRowAccess(document.AccountId, nameof(Document), signature.DocumentId.ToString(), forWrite: true);
        await EnsureOwnerWriteAccessAsync(document.AccountId, document.OwnerEntityType, document.OwnerEntityId, cancellationToken);

        // A supplied signature image must itself be a servable Signature-category document in the account (AC11).
        if (signature.SignatureImageDocumentId.HasValue)
        {
            var image = await Context.Documents.FirstOrDefaultAsync(x => x.DocumentId == signature.SignatureImageDocumentId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Document), signature.SignatureImageDocumentId.Value.ToString());
            if (image.AccountId != document.AccountId
                || !string.Equals(image.Category, "Signature", StringComparison.OrdinalIgnoreCase)
                || !DocumentScanStatuses.IsServable(image.ScanStatus))
            {
                throw new ValidationException([new FluentValidation.Results.ValidationFailure(nameof(signature.SignatureImageDocumentId), "Signature image must be a servable document of category 'Signature' in the same account.")]);
            }
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new DocumentSignature(document.DocumentId, document.AccountId, signature.SignerPrincipalType, signature.SignerPrincipalId, signature.SignerName, signature.SignatureImageDocumentId, signature.LegalTextAccepted, signature.Latitude, signature.Longitude, now, now);
        await Context.DocumentSignatures.AddAsync(entity, cancellationToken);
        AddAuditEvent(document.AccountId, "SignDocument", "Document", document.DocumentId.ToString(), null, $$"""{"signer":{{Quote(signature.SignerName)}},"signerPrincipalId":{{Quote(signature.SignerPrincipalId)}}}""");
        await Context.SaveChangesAsync(cancellationToken);

        return new DocumentSignatureVm(entity.DocumentSignatureId, entity.DocumentId, entity.AccountId, entity.SignerPrincipalType, entity.SignerPrincipalId, entity.SignerName, entity.SignatureImageDocumentId, entity.LegalTextAccepted, entity.Latitude, entity.Longitude, entity.SignedAt, entity.CreatedAt);
    }

    public async Task<DocumentTypeVm> ConfigureDocumentTypeAsync(DocumentTypeDto documentType, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(documentType.AccountId);

        var existing = await Context.DocumentTypes
            .AsTracking().FirstOrDefaultAsync(t => t.AccountId == accountId && t.Category == documentType.Category, cancellationToken);
        if (existing is null)
        {
            var entity = new DocumentType(accountId, documentType.Category, documentType.DisplayName, documentType.Required, documentType.Expiring, documentType.DefaultValidityDays, enabled: true, DateTimeOffset.UtcNow);
            await Context.DocumentTypes.AddAsync(entity, cancellationToken);
            AddAuditEvent(accountId, "ConfigureDocumentType", "DocumentType", entity.DocumentTypeId.ToString(), null, DocumentTypeAuditValues(entity));
            await Context.SaveChangesAsync(cancellationToken);
            return ToTypeVm(entity);
        }

        var oldValues = DocumentTypeAuditValues(existing);
        existing.DisplayName = documentType.DisplayName;
        existing.Required = documentType.Required;
        existing.Expiring = documentType.Expiring;
        existing.DefaultValidityDays = documentType.DefaultValidityDays;
        existing.Enabled = true;
        AddAuditEvent(accountId, "ConfigureDocumentType", "DocumentType", existing.DocumentTypeId.ToString(), oldValues, DocumentTypeAuditValues(existing));
        await Context.SaveChangesAsync(cancellationToken);
        return ToTypeVm(existing);
    }

    public async Task DisableDocumentTypeAsync(Guid documentTypeId, CancellationToken cancellationToken)
    {
        var entity = await Context.DocumentTypes
            .AsTracking().FirstOrDefaultAsync(t => t.DocumentTypeId == documentTypeId, cancellationToken)
            ?? throw new NotFoundException(nameof(DocumentType), documentTypeId.ToString());
        RequireRowAccess(entity.AccountId, nameof(DocumentType), documentTypeId.ToString(), forWrite: true);
        entity.Enabled = false;
        AddAuditEvent(entity.AccountId, "DisableDocumentType", "DocumentType", entity.DocumentTypeId.ToString(), null, DocumentTypeAuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateDocumentAsync(Guid documentId, string action, Action<Document> update, CancellationToken cancellationToken, string? reason = null, bool revokeShares = false)
    {
        var entity = await Context.Documents
            .AsTracking().FirstOrDefaultAsync(x => x.DocumentId == documentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Document), documentId.ToString());
        RequireRowAccess(entity.AccountId, nameof(Document), documentId.ToString(), forWrite: true);
        var oldValues = AuditValues(entity);
        update(entity);
        if (revokeShares)
        {
            // Same save as the status change: a deleted or voided document must not stay reachable
            // through a public link for even one request.
            var resourceId = entity.DocumentId.ToString();
            var shares = await Context.PublicLinkGrants
                .AsTracking()
                .Where(g => g.AccountId == entity.AccountId && g.ResourceType == DocumentSharing.ResourceType && g.ResourceId == resourceId && g.RevokedAt == null)
                .ToListAsync(cancellationToken);
            foreach (var share in shares)
            {
                share.RevokedAt = DateTimeOffset.UtcNow;
                share.RevokedBy = ActorId();
            }
        }

        AddAuditEvent(entity.AccountId, action, "Document", entity.DocumentId.ToString(), oldValues, AuditValues(entity, reason));
        await Context.SaveChangesAsync(cancellationToken);
    }

    // What a principal may file: a staff only platform evidence, as an internal record with no
    // expiry; anyone else the account's enabled document types or a platform module category.
    private async Task<DocumentDto> ValidateUploadAsync(Guid accountId, DocumentDto document, CancellationToken cancellationToken)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        var classification = DocumentClassifications.Normalize(document.Classification);
        if (classification is null)
        {
            failures.Add(new(nameof(document.Classification), "Unknown classification."));
        }

        if (!string.Equals(document.VisibilityScope?.Trim(), DocumentVisibilityScopes.Owner, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add(new(nameof(document.VisibilityScope), "Unknown visibility scope."));
        }

        if (document.ExpiresAt is { } expiresAt && expiresAt <= DateTimeOffset.UtcNow)
        {
            failures.Add(new(nameof(document.ExpiresAt), "The expiry date must be in the future."));
        }

        if (Principal.PrincipalType == PrincipalType.Staff)
        {
            if (!PlatformDocumentCategories.StaffUploadable.Contains(document.Category))
            {
                failures.Add(new(nameof(document.Category), "Staff may not file documents in this category."));
            }

            if (classification is not null && classification != DocumentClassifications.Internal)
            {
                failures.Add(new(nameof(document.Classification), "Staff file internal documents only."));
            }

            if (document.ExpiresAt is not null)
            {
                failures.Add(new(nameof(document.ExpiresAt), "Staff may not set an expiry date."));
            }
        }
        else if (!PlatformDocumentCategories.All.Contains(document.Category)
            && !await Context.DocumentTypes.AnyAsync(t => t.AccountId == accountId && t.Enabled && t.Category == document.Category, cancellationToken))
        {
            failures.Add(new(nameof(document.Category), "The category is not a document type of this account."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return document with { Classification = classification!, VisibilityScope = DocumentVisibilityScopes.Owner };
    }

    private async Task EnsureOwnerWriteAccessAsync(Guid accountId, string ownerEntityType, string ownerEntityId, CancellationToken cancellationToken)
    {
        // The owner resolver validates existence + account match + visibility/assignment. It must run for
        // EVERYONE on registered owner types so a cross-account owner reference is rejected even for
        // admins/managers.
        if (await accessPolicy.CanAccessOwnerAsync(accountId, ownerEntityType, ownerEntityId, forWrite: true, cancellationToken))
        {
            return;
        }

        // Unregistered (module-owned) owner types have no resolver yet: only trusted service/privileged
        // identities may register metadata for them; plain users and staff are denied.
        if (!DocumentOwnerTypes.IsRegistered(ownerEntityType) && accessPolicy.IsPrivilegedPrincipal)
        {
            return;
        }

        throw new ForbiddenAccessException("Insufficient permissions to write documents for this owner.");
    }

    private Document BuildDocument(Guid accountId, DocumentDto document)
    {
        var entity = new Document(accountId, document.OwnerEntityType, document.OwnerEntityId, document.UploadedByPrincipalType, document.UploadedByPrincipalId, document.StorageProvider, document.StorageKey, document.ContentType, document.SizeBytes, document.Sha256Hash, document.Classification, document.Status, document.ExpiresAt, document.VisibilityScope, document.ScanStatus, document.FileName, document.Category, document.Title, document.Description)
        {
            CurrentVersion = 1,
            CapturedLatitude = document.CapturedLatitude,
            CapturedLongitude = document.CapturedLongitude,
            CapturedAtDeviceTime = document.CapturedAtDeviceTime,
            SourceDeviceRegistrationId = document.SourceDeviceRegistrationId
        };
        return entity;
    }

    private void AddDocumentVersion(Document entity, string storageProvider, string storageKey, string sha256Hash, long sizeBytes, string contentType, string fileName, string scanStatus, string? reason)
    {
        var version = new DocumentVersion(entity.DocumentId, entity.AccountId, entity.CurrentVersion, storageProvider, storageKey, sha256Hash, sizeBytes, contentType, fileName, scanStatus, Principal.PrincipalType.ToString(), ActorId(), reason, DateTimeOffset.UtcNow);
        Context.DocumentVersions.Add(version);
    }

    private string ActorId()
        => Principal.UserId?.ToString() ?? Principal.StaffId?.ToString() ?? Principal.ClientId ?? Principal.SubjectId ?? "unknown";

    private DocumentVm ToVm(Document x)
    {
        var downloadUrl = DocumentScanStatuses.IsServable(x.ScanStatus)
            ? $"/documents/{x.DocumentId}/download"
            : null;
        return new DocumentVm(x.DocumentId, x.AccountId, x.OwnerEntityType, x.OwnerEntityId, x.UploadedByPrincipalType, x.UploadedByPrincipalId, x.FileName, x.Category, x.Title, x.Description, x.ContentType, x.SizeBytes, x.Sha256Hash, x.Classification, x.Status, x.ExpiresAt, x.VisibilityScope, x.ScanStatus, x.CurrentVersion, downloadUrl, x.LastModified);
    }

    private static DocumentTypeVm ToTypeVm(DocumentType t)
        => new(t.DocumentTypeId, t.AccountId, t.Category, t.DisplayName, t.Required, t.Expiring, t.DefaultValidityDays, t.Enabled, t.CreatedAt);

    private static string AuditValues(Document x, string? reason = null)
        => $$"""{"status":{{Quote(x.Status)}},"scanStatus":{{Quote(x.ScanStatus)}},"category":{{Quote(x.Category)}},"classification":{{Quote(x.Classification)}},"currentVersion":{{x.CurrentVersion}},"expiresAt":{{Quote(x.ExpiresAt)}},"reason":{{Quote(reason)}}}""";

    private static string DocumentTypeAuditValues(DocumentType t)
        => $$"""{"category":{{Quote(t.Category)}},"required":{{(t.Required ? "true" : "false")}},"expiring":{{(t.Expiring ? "true" : "false")}},"enabled":{{(t.Enabled ? "true" : "false")}}}""";
}
