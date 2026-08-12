namespace TrackHub.Manager.Application.AlertEvents.Queries;

[Authorize(Resource = Resources.Alerts, Action = Actions.Read)]
public readonly record struct GetAlertEventsQuery(Guid AccountId, DateTimeOffset? From = null, DateTimeOffset? To = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<AlertEventVm>>;
public class GetAlertEventsQueryHandler(IAlertEventReader reader) : IRequestHandler<GetAlertEventsQuery, IReadOnlyCollection<AlertEventVm>>
{
    public async Task<IReadOnlyCollection<AlertEventVm>> Handle(GetAlertEventsQuery request, CancellationToken cancellationToken) => await reader.GetAlertEventsAsync(request.AccountId, request.From, request.To, request.Skip, request.Take, cancellationToken);
}
