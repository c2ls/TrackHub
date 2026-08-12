using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

// Account-configurable document type (Category) with required/expiring flags. Drives
// the type-config UI and the "missing required documents" report.
public sealed class DocumentType(Guid accountId, string category, string? displayName, bool required, bool expiring, int? defaultValidityDays, bool enabled, DateTimeOffset createdAt) : BaseEntity
{
    public Guid DocumentTypeId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public string Category { get; set; } = category;
    public string? DisplayName { get; set; } = displayName;
    public bool Required { get; set; } = required;
    public bool Expiring { get; set; } = expiring;
    public int? DefaultValidityDays { get; set; } = defaultValidityDays;
    public bool Enabled { get; set; } = enabled;
    public DateTimeOffset CreatedAt { get; set; } = createdAt;
}
