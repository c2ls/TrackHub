namespace TrackHub.Reporting.Domain.Models.Manager;

public readonly record struct ManagerTransporterVm(
    Guid TransporterId,
    string Name);
