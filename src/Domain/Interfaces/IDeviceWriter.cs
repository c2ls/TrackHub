using TrackHub.Manager.Domain.Records;

namespace TrackHub.Manager.Domain.Interfaces;

public interface IDeviceWriter
{
    Task<DeviceVm> UpsertSynchronizedDeviceAsync(DeviceDto deviceDto, CancellationToken cancellationToken);
    // Manual registration for providers without a device-catalog API (Prosegur).
    Task<DeviceVm> CreateManualDeviceAsync(DeviceDto deviceDto, CancellationToken cancellationToken);
    Task SetDetectedStatusAsync(Guid deviceId, DetectedStatus status, CancellationToken cancellationToken);
    Task DeleteDeviceAsync(Guid deviceId, CancellationToken cancellationToken);
    Task<int> DeleteDevicesByOperatorAsync(Guid operatorId, CancellationToken cancellationToken);
}
