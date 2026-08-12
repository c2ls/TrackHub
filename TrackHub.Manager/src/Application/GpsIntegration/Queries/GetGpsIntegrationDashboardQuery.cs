namespace TrackHub.Manager.Application.GpsIntegration.Queries;

[Authorize(Resource = Resources.GpsIntegrationDashboard, Action = Actions.Read)]

public readonly record struct GetGpsIntegrationDashboardQuery(Guid AccountId) : IRequest<GpsIntegrationDashboardVm>;

public class GetGpsIntegrationDashboardQueryHandler(IGpsIntegrationDashboardReader reader)
    : IRequestHandler<GetGpsIntegrationDashboardQuery, GpsIntegrationDashboardVm>
{
    public Task<GpsIntegrationDashboardVm> Handle(GetGpsIntegrationDashboardQuery request, CancellationToken cancellationToken)
        => reader.GetAsync(request.AccountId, cancellationToken);
}
