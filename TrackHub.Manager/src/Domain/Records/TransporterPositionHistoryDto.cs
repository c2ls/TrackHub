namespace TrackHub.Manager.Domain.Records;

public readonly record struct TransporterPositionHistoryDto(
    Guid AccountId,
    Guid OperatorId,
    Guid DeviceId,
    Guid TransporterId,
    DateTimeOffset SourceTimestamp,
    double Latitude,
    double Longitude,
    double? Altitude,
    double Speed,
    double? Course,
    int? EventId,
    string? Address,
    string? City,
    string? State,
    string? Country,
    string? Attributes,
    string IdempotencyKey);
