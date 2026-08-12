namespace TrackHub.Manager.Domain.Models;

public readonly record struct CredentialMetadataVm(
    Guid CredentialId,
    Guid OperatorId,
    string Uri,
    string UsernameMask,
    bool HasPassword,
    bool HasKey,
    bool HasKey2,
    bool HasToken,
    bool HasRefreshToken,
    DateTimeOffset? TokenExpiration,
    DateTimeOffset? RefreshTokenExpiration,
    int CredentialVersion,
    DateTimeOffset? RotatedAt,
    string? RotatedByPrincipalType,
    string? RotatedByPrincipalId);
