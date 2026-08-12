namespace TrackHub.Manager.Domain.Models;

public readonly record struct DocumentTypeVm(
    Guid DocumentTypeId,
    Guid AccountId,
    string Category,
    string? DisplayName,
    bool Required,
    bool Expiring,
    int? DefaultValidityDays,
    bool Enabled,
    DateTimeOffset CreatedAt);
