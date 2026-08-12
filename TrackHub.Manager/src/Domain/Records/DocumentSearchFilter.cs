namespace TrackHub.Manager.Domain.Records;

// Global admin library search filters. Group-scoped for non-admins in the reader.
public readonly record struct DocumentSearchFilter(
    string? OwnerEntityType = null,
    string? OwnerEntityId = null,
    string? Category = null,
    string? Classification = null,
    string? Status = null,
    int? ExpiringWithinDays = null,
    string? UploadedByPrincipalId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null);
