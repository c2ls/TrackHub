using TrackHub.Telemetry.Application.GpsIntegration.Queries;

namespace TrackHub.Telemetry.Web.GraphQL.Query;

public partial class Query
{
    public async Task<PlatformSyncActivityVm> GetPlatformSyncActivity([Service] ISender sender, [AsParameters] GetPlatformSyncActivityQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);
}
