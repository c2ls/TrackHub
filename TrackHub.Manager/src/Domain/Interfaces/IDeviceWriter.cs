using TrackHub.Manager.Domain.Records;

namespace TrackHub.Manager.Domain.Interfaces;

public interface IDeviceWriter
{
    Task<DeviceVm> UpsertSynchronizedDeviceAsync(DeviceDto deviceDto, CancellationToken cancellationToken);
    Task SetDetectedStatusAsync(Guid deviceId, DetectedStatus status, CancellationToken cancellationToken);
    Task DeleteDeviceAsync(Guid deviceId, CancellationToken cancellationToken);
    Task<int> DeleteDevicesByOperatorAsync(Guid operatorId, CancellationToken cancellationToken);
}
