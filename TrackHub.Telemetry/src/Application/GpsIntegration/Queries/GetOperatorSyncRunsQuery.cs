using Common.Domain.Helpers;

namespace TrackHub.Telemetry.Application.GpsIntegration.Queries;

[Authorize(Resource = Resources.OperatorSyncRuns, Action = Actions.Read)]

public readonly record struct GetOperatorSyncRunsQuery(Guid? AccountId, Guid? OperatorId, int Take = 50) : IRequest<IReadOnlyCollection<OperatorSyncRunVm>>;

public class GetOperatorSyncRunsQueryHandler(IOperatorSyncRunReader reader)
    : IRequestHandler<GetOperatorSyncRunsQuery, IReadOnlyCollection<OperatorSyncRunVm>>
{
    public Task<IReadOnlyCollection<OperatorSyncRunVm>> Handle(GetOperatorSyncRunsQuery request, CancellationToken cancellationToken)
    {
        var dict = new Dictionary<string, object>();
        if (request.AccountId.HasValue) dict[nameof(OperatorSyncRunVm.AccountId)] = request.AccountId.Value;
        if (request.OperatorId.HasValue) dict[nameof(OperatorSyncRunVm.OperatorId)] = request.OperatorId.Value;
        return reader.GetAsync(new Filters(dict), request.Take, cancellationToken);
    }
}
