namespace TrackHub.Manager.Application.AccountFeatures.Commands;

[Authorize(Resource = Resources.AccountFeatures, Action = Actions.Write)]
public readonly record struct SetAccountFeatureCommand(AccountFeatureDto Feature) : IRequest<AccountFeatureVm>;
public class SetAccountFeatureCommandHandler(IAccountFeatureWriter writer) : IRequestHandler<SetAccountFeatureCommand, AccountFeatureVm>
{
    public async Task<AccountFeatureVm> Handle(SetAccountFeatureCommand request, CancellationToken cancellationToken) => await writer.SetAccountFeatureAsync(request.Feature, cancellationToken);
}

[Authorize(Resource = Resources.AccountFeatures, Action = Actions.Delete)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct DisableAccountFeatureCommand(Guid AccountFeatureId) : IRequest;
public class DisableAccountFeatureCommandHandler(IAccountFeatureWriter writer) : IRequestHandler<DisableAccountFeatureCommand>
{
    public async Task Handle(DisableAccountFeatureCommand request, CancellationToken cancellationToken) => await writer.DisableAccountFeatureAsync(request.AccountFeatureId, cancellationToken);
}

[Authorize(Resource = Resources.AccountFeatures, Action = Actions.Edit)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct UpdateAccountFeatureConfigurationCommand(Guid AccountFeatureId, string? ConfigurationJson) : IRequest;
public class UpdateAccountFeatureConfigurationCommandHandler(IAccountFeatureWriter writer) : IRequestHandler<UpdateAccountFeatureConfigurationCommand>
{
    public async Task Handle(UpdateAccountFeatureConfigurationCommand request, CancellationToken cancellationToken) => await writer.UpdateAccountFeatureConfigurationAsync(request.AccountFeatureId, request.ConfigurationJson, cancellationToken);
}
