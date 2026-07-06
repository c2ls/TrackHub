using Common.Domain.Helpers;

namespace TrackHub.Telemetry.Application.GpsIntegration.Queries;

[Authorize(Resource = Resources.PositionHistory, Action = Actions.Read)]
[RequireFeature(FeatureKeys.GpsPositionHistory)]
public readonly record struct GetPositionHistoryQuery(
    Guid AccountId,
    Guid? TransporterId = null,
    Guid? DeviceId = null,
    int Take = 500) : IRequest<IReadOnlyCollection<TransporterPositionHistoryVm>>;

public class GetPositionHistoryQueryHandler(ITransporterPositionHistoryReader reader)
    : IRequestHandler<GetPositionHistoryQuery, IReadOnlyCollection<TransporterPositionHistoryVm>>
{
    public Task<IReadOnlyCollection<TransporterPositionHistoryVm>> Handle(GetPositionHistoryQuery request, CancellationToken cancellationToken)
    {
        var dict = new Dictionary<string, object>
        {
            [nameof(TransporterPositionHistoryVm.AccountId)] = request.AccountId
        };
        if (request.TransporterId.HasValue) dict[nameof(TransporterPositionHistoryVm.TransporterId)] = request.TransporterId.Value;
        if (request.DeviceId.HasValue) dict[nameof(TransporterPositionHistoryVm.DeviceId)] = request.DeviceId.Value;
        return reader.GetAsync(new Filters(dict), request.Take, cancellationToken);
    }
}
