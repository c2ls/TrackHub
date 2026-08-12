namespace TrackHub.Manager.Application.AuditEvents.Queries;

[Authorize(Resource = Resources.Audit, Action = Actions.Read)]
public readonly record struct GetAuditTrailQuery(Guid AccountId, DateTimeOffset? From = null, DateTimeOffset? To = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<AuditEventVm>>;
public class GetAuditTrailQueryHandler(IAuditEventReader reader) : IRequestHandler<GetAuditTrailQuery, IReadOnlyCollection<AuditEventVm>>
{
    public async Task<IReadOnlyCollection<AuditEventVm>> Handle(GetAuditTrailQuery request, CancellationToken cancellationToken) => await reader.GetAuditTrailAsync(request.AccountId, request.From, request.To, request.Skip, request.Take, cancellationToken);
}
