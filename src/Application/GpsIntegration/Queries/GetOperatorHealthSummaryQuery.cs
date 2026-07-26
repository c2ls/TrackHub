namespace TrackHub.Telemetry.Application.GpsIntegration.Queries;

// Operator health is core: readable by authorized users regardless of
// account features; only the background health loop is feature-scoped.
[Authorize(Resource = Resources.OperatorHealth, Action = Actions.Read)]
[AccountScopeEnforcedInHandler]
public readonly record struct GetOperatorHealthSummaryQuery(Guid OperatorId, int LookbackHours = 24) : IRequest<OperatorHealthSummaryVm>;

public class GetOperatorHealthSummaryQueryHandler(IOperatorHealthCheckReader reader)
    : IRequestHandler<GetOperatorHealthSummaryQuery, OperatorHealthSummaryVm>
{
    public Task<OperatorHealthSummaryVm> Handle(GetOperatorHealthSummaryQuery request, CancellationToken cancellationToken)
    {
        var hours = request.LookbackHours <= 0 ? 24 : Math.Min(request.LookbackHours, 24 * 90);
        return reader.GetSummaryAsync(request.OperatorId, DateTimeOffset.UtcNow.AddHours(-hours), cancellationToken);
    }
}
