using TrackHub.Manager.Application.Principals.Queries;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<CurrentPrincipalVm> GetCurrentPrincipal([Service] ISender sender, CancellationToken cancellationToken) => await sender.Send(new GetCurrentPrincipalQuery(), cancellationToken);
}
