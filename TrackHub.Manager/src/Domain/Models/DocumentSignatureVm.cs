namespace TrackHub.Manager.Domain.Models;

public readonly record struct DocumentSignatureVm(
    Guid DocumentSignatureId,
    Guid DocumentId,
    Guid AccountId,
    string SignerPrincipalType,
    string SignerPrincipalId,
    string SignerName,
    Guid? SignatureImageDocumentId,
    string LegalTextAccepted,
    double? Latitude,
    double? Longitude,
    DateTimeOffset SignedAt,
    DateTimeOffset CreatedAt);
