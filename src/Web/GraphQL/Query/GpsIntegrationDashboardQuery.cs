using TrackHub.Manager.Application.GpsIntegration.Queries;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<GpsIntegrationDashboardVm> GetGpsIntegrationDashboard([Service] ISender sender, [AsParameters] GetGpsIntegrationDashboardQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);
}
