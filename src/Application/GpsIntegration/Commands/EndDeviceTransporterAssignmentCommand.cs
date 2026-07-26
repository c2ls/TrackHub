namespace TrackHub.Manager.Application.GpsIntegration.Commands;

[Authorize(Resource = Resources.SynchronizedDevices, Action = Actions.Edit)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct EndDeviceTransporterAssignmentCommand(Guid AssignmentId, string? Reason) : IRequest;

public class EndDeviceTransporterAssignmentCommandHandler(ITransporterDeviceAssignmentWriter writer)
    : IRequestHandler<EndDeviceTransporterAssignmentCommand>
{
    public Task Handle(EndDeviceTransporterAssignmentCommand request, CancellationToken cancellationToken)
        => writer.EndAssignmentAsync(request.AssignmentId, request.Reason, cancellationToken);
}
