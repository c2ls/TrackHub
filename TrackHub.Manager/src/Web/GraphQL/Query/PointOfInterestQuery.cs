using TrackHub.Manager.Application.PointsOfInterest.Queries.GetByAccount;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<PointsOfInterestPageVm> GetPointsOfInterestByAccount([Service] ISender sender, [AsParameters] GetPointsOfInterestByAccountQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<IReadOnlyCollection<PointOfInterestLookupVm>> GetPointOfInterestLookup([Service] ISender sender, CancellationToken cancellationToken)
        => await sender.Send(new GetPointOfInterestLookupQuery(), cancellationToken);
}
