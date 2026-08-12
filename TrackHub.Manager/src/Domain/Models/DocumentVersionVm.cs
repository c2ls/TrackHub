namespace TrackHub.Manager.Domain.Models;

// Version history view model. Storage keys are never exposed.
public readonly record struct DocumentVersionVm(
    Guid DocumentVersionId,
    Guid DocumentId,
    Guid AccountId,
    int VersionNumber,
    string ContentType,
    string FileName,
    long SizeBytes,
    string Sha256Hash,
    string ScanStatus,
    string? ReplacedByPrincipalType,
    string? ReplacedByPrincipalId,
    string? Reason,
    DateTimeOffset CreatedAt);
