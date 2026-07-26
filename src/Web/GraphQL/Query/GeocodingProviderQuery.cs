using TrackHub.Manager.Application.GeocodingProviders.Queries.Get;
using TrackHub.Manager.Application.GeocodingProviders.Queries.GetActive;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<IReadOnlyCollection<GeocodingProviderVm>> GetGeocodingProviders([Service] ISender sender, CancellationToken cancellationToken)
        => await sender.Send(new GetGeocodingProvidersQuery(), cancellationToken);

    public async Task<GeocodingProviderTokenVm?> GetActiveGeocodingProvider([Service] ISender sender, CancellationToken cancellationToken)
        => await sender.Send(new GetActiveGeocodingProviderQuery(), cancellationToken);
}
