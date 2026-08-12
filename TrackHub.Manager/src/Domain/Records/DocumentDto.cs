namespace TrackHub.Manager.Domain.Records;

// Carries FileName/Category (required for a usable library + download Content-Disposition),
// optional Title/Description, and optional mobile watermark fields. For the upload endpoint,
// StorageKey/Sha256Hash/SizeBytes/ScanStatus are server-computed and ignored if supplied.
public readonly record struct DocumentDto(
    Guid AccountId,
    string OwnerEntityType,
    string OwnerEntityId,
    string UploadedByPrincipalType,
    string UploadedByPrincipalId,
    string StorageProvider,
    string StorageKey,
    string ContentType,
    long SizeBytes,
    string Sha256Hash,
    string Classification,
    string Status,
    DateTimeOffset? ExpiresAt,
    string VisibilityScope,
    string ScanStatus,
    string FileName = "",
    string Category = "",
    string? Title = null,
    string? Description = null,
    double? CapturedLatitude = null,
    double? CapturedLongitude = null,
    DateTimeOffset? CapturedAtDeviceTime = null,
    Guid? SourceDeviceRegistrationId = null);
