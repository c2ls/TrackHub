namespace TrackHub.Reporting.Domain.Models.Manager;

public readonly record struct ManagerTransporterPositionHistoryVm(
    Guid TransporterPositionHistoryId,
    Guid AccountId,
    Guid OperatorId,
    Guid DeviceId,
    Guid TransporterId,
    DateTimeOffset SourceTimestamp,
    DateTimeOffset ReceivedAt,
    double Latitude,
    double Longitude);
