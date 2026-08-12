using TrackHub.Manager.Application.Groups.Queries;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<bool> ValidateGroupVisibility([Service] ISender sender, [AsParameters] ValidateGroupVisibilityQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
}
