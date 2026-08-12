using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

// A point-in-time snapshot of a document's bytes. Replacing a document creates a new
// version and re-points Document.CurrentVersion; prior versions are retained for history and marked
// for byte cleanup after the retention window.
public sealed class DocumentVersion(Guid documentId, Guid accountId, int versionNumber, string storageProvider, string storageKey, string sha256Hash, long sizeBytes, string contentType, string fileName, string scanStatus, string? replacedByPrincipalType, string? replacedByPrincipalId, string? reason, DateTimeOffset createdAt) : BaseEntity
{
    public Guid DocumentVersionId { get; private set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; } = documentId;
    public Guid AccountId { get; set; } = accountId;
    public int VersionNumber { get; set; } = versionNumber;
    public string StorageProvider { get; set; } = storageProvider;
    public string StorageKey { get; set; } = storageKey;
    public string Sha256Hash { get; set; } = sha256Hash;
    public long SizeBytes { get; set; } = sizeBytes;
    public string ContentType { get; set; } = contentType;
    public string FileName { get; set; } = fileName;
    public string ScanStatus { get; set; } = scanStatus;
    public string? ReplacedByPrincipalType { get; set; } = replacedByPrincipalType;
    public string? ReplacedByPrincipalId { get; set; } = replacedByPrincipalId;
    public string? Reason { get; set; } = reason;
    public DateTimeOffset CreatedAt { get; set; } = createdAt;

    // Stamped by the byte-retention cleanup job when the stored bytes are deleted after the
    // retention window; the metadata row is retained for audit. Null = bytes still present.
    public DateTimeOffset? BytesPurgedAt { get; set; }
}
