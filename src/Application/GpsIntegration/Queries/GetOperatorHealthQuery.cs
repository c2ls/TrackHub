namespace TrackHub.Telemetry.Application.GpsIntegration.Queries;

// Operator health is core: readable by authorized users regardless of
// account features; only the background health loop is feature-scoped.
[Authorize(Resource = Resources.OperatorHealth, Action = Actions.Read)]
[AccountScopeEnforcedInHandler]
public readonly record struct GetOperatorHealthQuery(Guid OperatorId) : IRequest<OperatorHealthVm>;

public class GetOperatorHealthQueryHandler(IOperatorHealthCheckReader reader) : IRequestHandler<GetOperatorHealthQuery, OperatorHealthVm>
{
    public Task<OperatorHealthVm> Handle(GetOperatorHealthQuery request, CancellationToken cancellationToken)
        => reader.GetLatestHealthAsync(request.OperatorId, cancellationToken);
}
