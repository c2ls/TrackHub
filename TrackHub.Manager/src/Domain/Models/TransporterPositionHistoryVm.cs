namespace TrackHub.Manager.Domain.Models;

public readonly record struct TransporterPositionHistoryVm(
    Guid TransporterPositionHistoryId,
    Guid AccountId,
    Guid OperatorId,
    Guid DeviceId,
    Guid TransporterId,
    DateTimeOffset SourceTimestamp,
    DateTimeOffset ReceivedAt,
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
