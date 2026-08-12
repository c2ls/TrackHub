namespace TrackHub.Manager.Application.GpsIntegration.Commands;

[Authorize(Resource = Resources.SynchronizedDevices, Action = Actions.Edit)]

// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct SetSynchronizedDeviceIgnoredCommand(Guid DeviceId, bool Ignored) : IRequest;

public class SetSynchronizedDeviceIgnoredCommandHandler(IDeviceWriter writer)
    : IRequestHandler<SetSynchronizedDeviceIgnoredCommand>
{
    public Task Handle(SetSynchronizedDeviceIgnoredCommand request, CancellationToken cancellationToken)
        => writer.SetDetectedStatusAsync(request.DeviceId,
            request.Ignored ? DetectedStatus.Ignored : DetectedStatus.Available, cancellationToken);
}
