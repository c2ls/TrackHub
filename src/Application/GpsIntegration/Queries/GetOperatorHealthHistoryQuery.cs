namespace TrackHub.Telemetry.Application.GpsIntegration.Queries;

// Operator health is core (spec 07 §3): readable by authorized users regardless of
// account features; only the background health loop is feature-scoped.
[Authorize(Resource = Resources.OperatorHealth, Action = Actions.Read)]
public readonly record struct GetOperatorHealthHistoryQuery(Guid OperatorId, int Take = 50) : IRequest<IReadOnlyCollection<OperatorHealthCheckVm>>;

public class GetOperatorHealthHistoryQueryHandler(IOperatorHealthCheckReader reader)
    : IRequestHandler<GetOperatorHealthHistoryQuery, IReadOnlyCollection<OperatorHealthCheckVm>>
{
    public Task<IReadOnlyCollection<OperatorHealthCheckVm>> Handle(GetOperatorHealthHistoryQuery request, CancellationToken cancellationToken)
        => reader.GetByOperatorAsync(request.OperatorId, request.Take, cancellationToken);
}
