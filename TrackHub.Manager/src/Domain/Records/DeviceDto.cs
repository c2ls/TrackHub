namespace TrackHub.Manager.Domain.Records;

public readonly record struct DeviceDto(
    Guid AccountId,
    Guid OperatorId,
    string Serial,
    string Name,
    int Identifier,
    string? ProviderDisplayName,
    short DeviceTypeId,
    string? Description,
    string? ProviderMetadataHash,
    string? ProviderStatus);
