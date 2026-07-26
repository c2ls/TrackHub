using Common.Application.Interfaces;
using Common.Mediator;
using Microsoft.EntityFrameworkCore;
using TrackHub.Manager.Application.Documents.Commands;
using TrackHub.Manager.Application.Documents.Queries;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.ManagerDB;
using TrackHub.Manager.Infrastructure.ManagerDB.Storage;

namespace TrackHub.Manager.Web.Endpoints;

// File-byte surfaces for documents. Metadata still flows through GraphQL; these REST
// endpoints stream/redirect bytes. Authorization is enforced by the dispatched commands/queries
// (User/Driver/ServiceClient) except the anonymous token-validated public download.
public sealed class Documents : Common.Web.Infrastructure.EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapPost("~/documents/upload", Upload).DisableAntiforgery();
        app.MapPost("~/documents/{documentId:guid}/versions", UploadVersion).DisableAntiforgery();
        app.MapGet("~/documents/{documentId:guid}/download", Download);
        app.MapGet("~/documents/public/{publicLinkGrantId:guid}", PublicDownload);
    }

    // POST ~/documents/upload — multipart. User or Driver. Streams to storage under a server-generated
    // key while computing SHA-256, creates the document Quarantined, and returns the redacted VM.
    public static async Task<IResult> Upload(HttpRequest request, ISender sender, IDocumentStorage storage, IUser user, ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return Results.BadRequest("Expected multipart/form-data.");
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("file") ?? (form.Files.Count > 0 ? form.Files[0] : null);
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest("Missing file.");
        }

        if (file.Length > DocumentLimits.DefaultMaxBytes)
        {
            return Results.BadRequest($"File exceeds the {DocumentLimits.DefaultMaxBytes / (1024 * 1024)} MB limit.");
        }

        var accountId = user.AccountId ?? ParseGuid(form["accountId"]);
        if (accountId is null || accountId == Guid.Empty)
        {
            return Results.BadRequest("Missing account id.");
        }

        var ownerEntityType = form["ownerEntityType"].ToString();
        var ownerEntityId = form["ownerEntityId"].ToString();
        var category = form["category"].ToString();
        if (string.IsNullOrWhiteSpace(ownerEntityType) || string.IsNullOrWhiteSpace(ownerEntityId) || string.IsNullOrWhiteSpace(category))
        {
            return Results.BadRequest("Missing ownerEntityType, ownerEntityId, or category.");
        }

        var fileName = string.IsNullOrWhiteSpace(form["fileName"]) ? file.FileName : form["fileName"].ToString();
        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        var classification = string.IsNullOrWhiteSpace(form["classification"]) ? DocumentClassifications.Internal : form["classification"].ToString();
        var visibilityScope = string.IsNullOrWhiteSpace(form["visibilityScope"]) ? "Owner" : form["visibilityScope"].ToString();

        var documentId = Guid.NewGuid();
        var storageKey = DocumentStorageKey.For(accountId.Value, ownerEntityType, documentId, 1);

        StoredObject stored;
        await using (var stream = file.OpenReadStream())
        {
            stored = await storage.SaveAsync(storageKey, stream, contentType, cancellationToken);
        }

        var dto = new DocumentDto(
            accountId.Value, ownerEntityType, ownerEntityId,
            user.PrincipalType.ToString(), ActorId(user),
            storage.Provider, storageKey, contentType, stored.SizeBytes, stored.Sha256Hash,
            classification, DocumentStatuses.Uploaded, ParseDate(form["expiresAt"]), visibilityScope, DocumentScanStatuses.Quarantined,
            fileName, category,
            NullIfEmpty(form["title"]), NullIfEmpty(form["description"]),
            ParseDouble(form["capturedLatitude"]), ParseDouble(form["capturedLongitude"]),
            ParseDate(form["capturedAtDeviceTime"]), ParseGuid(form["sourceDeviceRegistrationId"]));

        try
        {
            var vm = await sender.Send(new RegisterUploadedDocumentCommand(documentId, dto), cancellationToken);
            return Results.Ok(vm);
        }
        catch
        {
            // Authorization/validation failed after the bytes landed: best-effort cleanup, then rethrow
            // so the exception handler maps the real status (403/400/…).
            await storage.DeleteAsync(storageKey, cancellationToken);
            throw;
        }
    }

    // POST ~/documents/{id}/versions — multipart. User (Documents/Edit). Streams new bytes then re-points.
    public static async Task<IResult> UploadVersion(Guid documentId, HttpRequest request, ISender sender, IDocumentStorage storage, CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return Results.BadRequest("Expected multipart/form-data.");
        }

        // Authorizes the caller and yields the current version to compute the next one.
        var current = await sender.Send(new GetDocumentQuery(documentId), cancellationToken);

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("file") ?? (form.Files.Count > 0 ? form.Files[0] : null);
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest("Missing file.");
        }
        if (file.Length > DocumentLimits.DefaultMaxBytes)
        {
            return Results.BadRequest($"File exceeds the {DocumentLimits.DefaultMaxBytes / (1024 * 1024)} MB limit.");
        }

        var nextVersion = current.CurrentVersion + 1;
        var fileName = string.IsNullOrWhiteSpace(form["fileName"]) ? file.FileName : form["fileName"].ToString();
        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        var storageKey = DocumentStorageKey.For(current.AccountId, current.OwnerEntityType, documentId, nextVersion);

        StoredObject stored;
        await using (var stream = file.OpenReadStream())
        {
            stored = await storage.SaveAsync(storageKey, stream, contentType, cancellationToken);
        }

        var versionDto = new DocumentVersionDto(documentId, storage.Provider, storageKey, stored.Sha256Hash, stored.SizeBytes, contentType, fileName, NullIfEmpty(form["reason"]));
        try
        {
            var vm = await sender.Send(new ReplaceDocumentVersionCommand(documentId, versionDto), cancellationToken);
            return Results.Ok(vm);
        }
        catch
        {
            await storage.DeleteAsync(storageKey, cancellationToken);
            throw;
        }
    }

    // GET ~/documents/{id}/download — User/Driver/ServiceClient. Authorization + Clean gate enforced by
    // the query + reader. S3 → 302 presigned; local FS → stream with Content-Disposition.
    public static async Task<IResult> Download(Guid documentId, ISender sender, IDocumentStorage storage, ApplicationDbContext context, IUser user, CancellationToken cancellationToken)
    {
        var vm = await sender.Send(new GetDocumentQuery(documentId), cancellationToken);

        if (!string.Equals(vm.ScanStatus, DocumentScanStatuses.Clean, StringComparison.OrdinalIgnoreCase))
        {
            // Non-Clean files are undownloadable. Admin quarantine access is out of slice.
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        // The scan verdict that matters is the CURRENT VERSION's — those are the bytes about to be
        // served. The document-level column is a denormalisation of it and is checked above; this
        // second read is the authoritative one, so a document row whose verdict disagrees with its
        // current version (however it got that way) cannot serve unscanned or infected bytes.
        var target = await context.Documents
            .Where(x => x.DocumentId == documentId)
            .Select(x => new
            {
                x.StorageKey,
                VersionScanStatus = context.DocumentVersions
                    .Where(v => v.DocumentId == x.DocumentId && v.VersionNumber == x.CurrentVersion)
                    .Select(v => v.ScanStatus)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (target is null || string.IsNullOrEmpty(target.StorageKey))
        {
            return Results.NotFound();
        }

        if (!string.Equals(target.VersionScanStatus, DocumentScanStatuses.Clean, StringComparison.OrdinalIgnoreCase))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var storageKey = target.StorageKey;

        // Sensitive-classification downloads are audited.
        if (DocumentClassifications.IsSensitive(vm.Classification))
        {
            context.AuditEvents.Add(new AuditEvent(vm.AccountId, user.PrincipalType.ToString(), ActorId(user), "DownloadDocument", "Document", documentId.ToString(), "Succeeded", null, $$"""{"classification":"{{vm.Classification}}"}""", null, null, null, user.CorrelationId));
            await context.SaveChangesAsync(cancellationToken);
        }

        var presigned = await storage.TryCreatePresignedDownloadUrlAsync(storageKey, vm.FileName, TimeSpan.FromMinutes(5), cancellationToken);
        if (presigned is not null)
        {
            return Results.Redirect(presigned.ToString());
        }

        var stream = await storage.OpenReadAsync(storageKey, cancellationToken);
        return Results.File(stream, vm.ContentType, vm.FileName);
    }

    // GET ~/documents/public/{grantId}?token=… — anonymous, token-validated share.
    public static async Task<IResult> PublicDownload(Guid publicLinkGrantId, Guid accountId, string resourceId, string token, ApplicationDbContext context, IDocumentStorage storage, CancellationToken cancellationToken)
    {
        if (accountId == Guid.Empty || string.IsNullOrWhiteSpace(resourceId) || string.IsNullOrWhiteSpace(token))
        {
            return Results.BadRequest();
        }

        var tokenHash = PublicLinkTokenHasher.Hash(token);
        var grant = await context.PublicLinkGrants.FirstOrDefaultAsync(x =>
            x.PublicLinkGrantId == publicLinkGrantId && x.AccountId == accountId
            && x.ResourceType == DocumentSharing.ResourceType && x.ResourceId == resourceId
            && x.SubjectTokenIdHash == tokenHash, cancellationToken);

        if (grant is null || grant.RevokedAt.HasValue || !HasScope(grant.Scopes, DocumentSharing.ReadScope))
        {
            return Results.NotFound();
        }
        if (grant.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return Results.StatusCode(StatusCodes.Status410Gone);
        }

        if (!Guid.TryParse(resourceId, out var documentId))
        {
            return Results.NotFound();
        }

        var document = await context.Documents.FirstOrDefaultAsync(x => x.DocumentId == documentId && x.AccountId == accountId, cancellationToken);
        if (document is null || !string.Equals(document.ScanStatus, DocumentScanStatuses.Clean, StringComparison.OrdinalIgnoreCase))
        {
            return Results.NotFound();
        }

        // Same authoritative check as the authenticated Download path — and it matters more here,
        // because this route is anonymous: a public link is exactly how infected bytes would reach
        // someone outside the account.
        var publicVersionScanStatus = await context.DocumentVersions
            .Where(v => v.DocumentId == document.DocumentId && v.VersionNumber == document.CurrentVersion)
            .Select(v => v.ScanStatus)
            .FirstOrDefaultAsync(cancellationToken);
        if (!string.Equals(publicVersionScanStatus, DocumentScanStatuses.Clean, StringComparison.OrdinalIgnoreCase))
        {
            return Results.NotFound();
        }

        // The context is NoTracking by default; attach so the access-count increment actually persists.
        context.PublicLinkGrants.Attach(grant);
        grant.AccessCount++;
        grant.LastAccessedAt = DateTimeOffset.UtcNow;
        context.AuditEvents.Add(new AuditEvent(
            grant.AccountId, "PublicLink", grant.PublicLinkGrantId.ToString(),
            "PublicLinkAccessed", "Document", documentId.ToString(),
            "Succeeded", null,
            $$"""{"scope":"{{DocumentSharing.ReadScope}}","accessCount":{{grant.AccessCount}}}""",
            null, null, null, null));
        await context.SaveChangesAsync(cancellationToken);

        var presigned = await storage.TryCreatePresignedDownloadUrlAsync(document.StorageKey, document.FileName, TimeSpan.FromMinutes(5), cancellationToken);
        if (presigned is not null)
        {
            return Results.Redirect(presigned.ToString());
        }

        var stream = await storage.OpenReadAsync(document.StorageKey, cancellationToken);
        return Results.File(stream, document.ContentType, document.FileName);
    }

    private static bool HasScope(string scopes, string requestedScope)
        => scopes.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(scope => string.Equals(scope, requestedScope, StringComparison.OrdinalIgnoreCase));

    private static string ActorId(IUser user)
        => user.UserId?.ToString() ?? user.DriverId?.ToString() ?? user.ClientId ?? user.SubjectId ?? "unknown";

    private static Guid? ParseGuid(string? value) => Guid.TryParse(value, out var g) ? g : null;
    private static double? ParseDouble(string? value) => double.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : null;
    private static DateTimeOffset? ParseDate(string? value) => DateTimeOffset.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var dt) ? dt : null;
    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
