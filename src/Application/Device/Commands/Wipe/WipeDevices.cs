namespace TrackHub.Manager.Application.Device.Commands.Wipe;

[Authorize(Resource = Resources.Devices, Action = Actions.Delete)]
[AllowCrossAccount("Router's device-sync loop wipes an operator's synchronized devices under the global router/SyncWorker identity with no account claim. Tenant callers are NOT unguarded by this: DeviceWriter.DeleteDevicesByOperatorAsync filters the affected rows on the caller's account unless the caller is a global service identity.")]
public readonly record struct WipeDevicesCommand(Guid OperatorId) : IRequest;

public class WipeDevicesCommandHandler(IDeviceWriter writer) : IRequestHandler<WipeDevicesCommand>
{
    public async Task Handle(WipeDevicesCommand request, CancellationToken cancellationToken)
        => await writer.DeleteDevicesByOperatorAsync(request.OperatorId, cancellationToken);
}
