using TrackHub.Manager.Application.AccountSupportGrants.Queries;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<AccountSupportGrantVm> GetSupportGrantStatus([Service] ISender sender, [AsParameters] GetSupportGrantStatusQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<AccountSupportGrantVm>> GetAccountSupportGrants([Service] ISender sender, [AsParameters] GetAccountSupportGrantsQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
}
