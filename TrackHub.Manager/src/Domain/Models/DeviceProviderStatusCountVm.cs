namespace TrackHub.Manager.Domain.Models;

public readonly record struct DeviceProviderStatusCountVm(
    Guid OperatorId,
    string OperatorName,
    DetectedStatus DetectedStatus,
    int Count);
