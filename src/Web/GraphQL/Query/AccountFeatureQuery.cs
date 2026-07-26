using TrackHub.Manager.Application.AccountFeatures.Queries.Get;
using TrackHub.Manager.Application.AccountFeatures.Queries.GetMaster;
using TrackHub.Manager.Application.AccountFeatures.Queries.Validate;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<IReadOnlyCollection<AccountFeatureVm>> GetAccountFeatures([Service] ISender sender, [AsParameters] GetAccountFeaturesQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<AccountFeatureVm>> GetAccountFeaturesMaster([Service] ISender sender, [AsParameters] GetAccountFeaturesMasterQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<AccountFeatureVm>> GetAllAccountFeaturesMaster([Service] ISender sender, CancellationToken cancellationToken) => await sender.Send(new GetAllAccountFeaturesMasterQuery(), cancellationToken);
    public async Task<bool> ValidateFeatureEnabled([Service] ISender sender, [AsParameters] ValidateFeatureEnabledQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
}
