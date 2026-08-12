using TrackHub.Manager.Application.PublicLinks.Queries;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<PublicLinkGrantVm> GetPublicLinkGrant([Service] ISender sender, [AsParameters] GetPublicLinkGrantQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<PublicLinkGrantVm>> GetPublicLinkGrantsByAccount([Service] ISender sender, [AsParameters] GetPublicLinkGrantsByAccountQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
}
