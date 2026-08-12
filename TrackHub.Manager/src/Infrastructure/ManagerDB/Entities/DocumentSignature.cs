using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

// Tamper-evident record that a signature/consent occurred. Evidence only — not a legal
// e-signature workflow or third-party provider integration.
public sealed class DocumentSignature(Guid documentId, Guid accountId, string signerPrincipalType, string signerPrincipalId, string signerName, Guid? signatureImageDocumentId, string legalTextAccepted, double? latitude, double? longitude, DateTimeOffset signedAt, DateTimeOffset createdAt) : BaseEntity
{
    public Guid DocumentSignatureId { get; private set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; } = documentId;
    public Guid AccountId { get; set; } = accountId;
    public string SignerPrincipalType { get; set; } = signerPrincipalType;
    public string SignerPrincipalId { get; set; } = signerPrincipalId;
    public string SignerName { get; set; } = signerName;
    public Guid? SignatureImageDocumentId { get; set; } = signatureImageDocumentId;
    public string LegalTextAccepted { get; set; } = legalTextAccepted;
    public double? Latitude { get; set; } = latitude;
    public double? Longitude { get; set; } = longitude;
    public DateTimeOffset SignedAt { get; set; } = signedAt;
    public DateTimeOffset CreatedAt { get; set; } = createdAt;
}
