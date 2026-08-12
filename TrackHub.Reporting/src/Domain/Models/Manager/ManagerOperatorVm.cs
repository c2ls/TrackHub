namespace TrackHub.Reporting.Domain.Models.Manager;

public readonly record struct ManagerOperatorVm(
    Guid OperatorId,
    string Name,
    bool Enabled);
