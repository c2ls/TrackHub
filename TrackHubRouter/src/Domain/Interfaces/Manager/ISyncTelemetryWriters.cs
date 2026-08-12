using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Domain.Interfaces.Manager;

public interface IOperatorSyncRunWriter
{
    Task RecordAsync(OperatorSyncRunDto dto, CancellationToken cancellationToken);
}

public interface IOperatorHealthCheckWriter
{
    Task RecordAsync(OperatorHealthCheckDto dto, CancellationToken cancellationToken);
}

// Same telemetry write with the Router's own service identity, for user-triggered
// manual health checks (the Manager command is ServiceClient-only).
public interface IOperatorHealthCheckSystemWriter : IOperatorHealthCheckWriter;

public interface IDeviceSyncWriter
{
    Task ResetAsync(Guid accountId, Guid operatorId, CancellationToken cancellationToken);

    // Returns the device-sync counts so the Router can record exactly one sync run
    // per attempt; Manager no longer records the run itself.
    Task<DeviceSyncCountsVm> SynchronizeAsync(Guid accountId, Guid operatorId, IEnumerable<SynchronizedDeviceDto> devices, string correlationId, string triggerType, bool autoAssignNewDevices, CancellationToken cancellationToken);
}

public interface IAlertEventWriter
{
    Task RecordAsync(AlertEventDto dto, CancellationToken cancellationToken);
}
