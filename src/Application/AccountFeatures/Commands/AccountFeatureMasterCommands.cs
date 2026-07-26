namespace TrackHub.Manager.Application.AccountFeatures.Commands;

// Write twin of GetAccountFeaturesMasterQuery, which is already declared cross-account. The account
// is nested in AccountFeatureDto, so this side went unpoliced until TrackHubCommon 1.0.7.
[Authorize(Resource = Resources.AccountFeaturesMaster, Action = Actions.Write)]
[AllowCrossAccount("Master/platform administration surface (portal systemadmin console): an operator holding AccountFeaturesMaster/Write provisions the feature matrix OF another account, so the target account is necessarily not their own.")]
public readonly record struct SetAccountFeatureMasterCommand(AccountFeatureDto Feature) : IRequest<AccountFeatureVm>;
public class SetAccountFeatureMasterCommandHandler(IAccountFeatureMasterWriter writer) : IRequestHandler<SetAccountFeatureMasterCommand, AccountFeatureVm>
{
    public async Task<AccountFeatureVm> Handle(SetAccountFeatureMasterCommand request, CancellationToken cancellationToken) => await writer.SetAccountFeatureAsync(request.Feature, cancellationToken);
}
