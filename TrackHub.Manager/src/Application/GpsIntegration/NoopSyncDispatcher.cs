using TrackHub.Manager.Domain.Interfaces;

namespace TrackHub.Manager.Application.GpsIntegration;

/// Default dispatcher used only when the Router integration is not registered.
public sealed class NoopSyncDispatcher : ISyncDispatcher
{
    public Task<bool> DispatchManualSyncAsync(Guid accountId, Guid operatorId, string correlationId, bool resetDeviceCatalog, bool? autoAssignNewDevices, CancellationToken cancellationToken)
        => Task.FromResult(false);
}
