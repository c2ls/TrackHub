namespace TrackHub.Manager.Application.AccountFeatures.Queries.Get;

[Authorize(Resource = Resources.AccountFeatures, Action = Actions.Read)]
public readonly record struct GetAccountFeaturesQuery(Guid AccountId) : IRequest<IReadOnlyCollection<AccountFeatureVm>>;
public class GetAccountFeaturesQueryHandler(IAccountFeatureReader reader) : IRequestHandler<GetAccountFeaturesQuery, IReadOnlyCollection<AccountFeatureVm>>
{
    public async Task<IReadOnlyCollection<AccountFeatureVm>> Handle(GetAccountFeaturesQuery request, CancellationToken cancellationToken) 
        => await reader.GetAccountFeaturesAsync(request.AccountId, cancellationToken);
}


