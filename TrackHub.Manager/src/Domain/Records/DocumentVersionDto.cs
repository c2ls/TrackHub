namespace TrackHub.Manager.Domain.Records;

// New version metadata for a replace. Bytes arrive via the upload endpoint, which
// server-computes StorageKey/Sha256Hash/SizeBytes before dispatching the replace command.
public readonly record struct DocumentVersionDto(
    Guid DocumentId,
    string StorageProvider,
    string StorageKey,
    string Sha256Hash,
    long SizeBytes,
    string ContentType,
    string FileName,
    string? Reason = null);
