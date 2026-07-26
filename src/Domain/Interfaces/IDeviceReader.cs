using TrackHub.Manager.Domain.Records;

namespace TrackHub.Manager.Domain.Interfaces;

public interface IDeviceReader
{
    Task<DeviceVm> GetDeviceAsync(Guid id, CancellationToken cancellationToken);
    Task<DevicesPageVm> GetDevicesByAccountAsync(Guid accountId, int skip, int take, string? search, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DeviceVm>> GetDevicesByOperatorAsync(Guid operatorId, CancellationToken cancellationToken);
    Task<DevicesPageVm> GetUnassignedDevicesAsync(Guid accountId, int skip, int take, string? search, CancellationToken cancellationToken);
    Task<DevicesPageVm> SearchDevicesAsync(Guid accountId, SynchronizedDeviceFilter filter, int skip, int take, string? search, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DeviceLookupVm>> GetDeviceLookupByAccountAsync(Guid accountId, int fetchSize, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<(string Serial, Guid OperatorId)>> FindDuplicateSerialsAsync(
        Guid accountId, Guid excludeOperatorId, IReadOnlyCollection<string> serials, CancellationToken cancellationToken);
}
