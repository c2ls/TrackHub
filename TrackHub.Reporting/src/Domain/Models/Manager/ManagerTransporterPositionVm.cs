namespace TrackHub.Reporting.Domain.Models.Manager;

public readonly record struct ManagerTransporterPositionVm(
    Guid TransporterId,
    string DeviceName,
    DateTimeOffset DeviceDateTime);
