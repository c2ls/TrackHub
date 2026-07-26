namespace TrackHub.Manager.Application.AccountFeatures.Queries.GetMaster;

[Authorize(Resource = Resources.AccountFeaturesMaster, Action = Actions.Read)]
[AllowCrossAccount("Master/platform administration surface: an operator holding AccountFeaturesMaster/Read provisions the feature matrix OF another account, so the target account is necessarily not their own.")]
public readonly record struct GetAccountFeaturesMasterQuery(Guid AccountId) : IRequest<IReadOnlyCollection<AccountFeatureVm>>;

public class GetAccountFeaturesMasterQueryHandler(IAccountFeatureMasterReader reader) : IRequestHandler<GetAccountFeaturesMasterQuery, IReadOnlyCollection<AccountFeatureVm>>
{
    public async Task<IReadOnlyCollection<AccountFeatureVm>> Handle(GetAccountFeaturesMasterQuery request, CancellationToken cancellationToken)
        => await reader.GetAccountFeaturesAsync(request.AccountId, cancellationToken);
}

/// <summary>
/// Every account's features in one call — the batched variant schedulers (Router SyncWorker)
/// and cross-account reports use instead of one accountFeaturesMaster call per account.
/// </summary>
[Authorize(Resource = Resources.AccountFeaturesMaster, Action = Actions.Read)]
[AllowCrossAccount("Cross-account feature snapshot for the SyncWorker device-sync loop and cross-account reports, called under global service identities with no account claim. The AccountFeaturesMaster resource gates access.")]
public readonly record struct GetAllAccountFeaturesMasterQuery() : IRequest<IReadOnlyCollection<AccountFeatureVm>>;

public class GetAllAccountFeaturesMasterQueryHandler(IAccountFeatureMasterReader reader) : IRequestHandler<GetAllAccountFeaturesMasterQuery, IReadOnlyCollection<AccountFeatureVm>>
{
    public async Task<IReadOnlyCollection<AccountFeatureVm>> Handle(GetAllAccountFeaturesMasterQuery request, CancellationToken cancellationToken)
        => await reader.GetAllAccountFeaturesAsync(cancellationToken);
}
