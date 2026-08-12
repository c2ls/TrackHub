using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

/// <summary>
/// The single entity covering licenses and every other credentialed qualification (spec 09 §18.1) —
/// one model, one expiration pipeline, one document-link pattern. <c>Driver.LicenseNumber</c>/
/// <c>Driver.LicenseExpiresAt</c> remain as the primary-license summary shown in driver lists.
/// </summary>
public sealed class DriverQualification(
    Guid accountId,
    Guid driverId,
    string qualificationType,
    string? category,
    string? number,
    DateOnly? issuedAt,
    DateOnly? expiresAt,
    string? issuingAuthority,
    string status,
    Guid? documentId,
    string? notes) : BaseAuditableEntity
{
    public Guid DriverQualificationId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public Guid DriverId { get; set; } = driverId;
    public string QualificationType { get; set; } = qualificationType;
    public string? Category { get; set; } = category;
    public string? Number { get; set; } = number;
    public DateOnly? IssuedAt { get; set; } = issuedAt;
    public DateOnly? ExpiresAt { get; set; } = expiresAt;
    public string? IssuingAuthority { get; set; } = issuingAuthority;
    public string Status { get; set; } = status;
    public Guid? DocumentId { get; set; } = documentId;
    public string? Notes { get; set; } = notes;
}
