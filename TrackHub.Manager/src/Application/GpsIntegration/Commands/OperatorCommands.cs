namespace TrackHub.Manager.Application.GpsIntegration.Commands;

[Authorize(Resource = Resources.Operators, Action = Actions.Edit)]

// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct SetOperatorEnabledCommand(Guid OperatorId, bool Enabled) : IRequest;
public class SetOperatorEnabledCommandHandler(IOperatorWriter writer) : IRequestHandler<SetOperatorEnabledCommand>
{
    public Task Handle(SetOperatorEnabledCommand request, CancellationToken cancellationToken)
        => writer.SetEnabledAsync(request.OperatorId, request.Enabled, cancellationToken);
}
