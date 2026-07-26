using Common.Application.Interfaces;

namespace TrackHub.Manager.Application.Notifications.Commands;

[Authorize(Resource = Resources.Notifications, Action = Actions.Write)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct CreateAlertSubscriptionCommand(AlertSubscriptionDto Subscription) : IRequest<AlertSubscriptionVm>;
public class CreateAlertSubscriptionCommandHandler(IAlertSubscriptionWriter writer, IFeatureFlagService featureFlags) : IRequestHandler<CreateAlertSubscriptionCommand, AlertSubscriptionVm>
{
    public async Task<AlertSubscriptionVm> Handle(CreateAlertSubscriptionCommand request, CancellationToken cancellationToken)
    {
        await NotificationChannelEntitlements.RequireConfiguredChannelsAsync(featureFlags, request.Subscription.AccountId, [request.Subscription.Channel], cancellationToken);
        return await writer.CreateAlertSubscriptionAsync(request.Subscription, cancellationToken);
    }
}
public class CreateAlertSubscriptionCommandValidator : AbstractValidator<CreateAlertSubscriptionCommand>
{
    public CreateAlertSubscriptionCommandValidator()
    {
        AlertSubscriptionContractRules.Apply(this, x => x.Subscription);
    }
}

[Authorize(Resource = Resources.Notifications, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct UpdateAlertSubscriptionCommand(Guid AlertSubscriptionId, AlertSubscriptionDto Subscription) : IRequest;
public class UpdateAlertSubscriptionCommandHandler(IAlertSubscriptionWriter writer, IFeatureFlagService featureFlags) : IRequestHandler<UpdateAlertSubscriptionCommand>
{
    public async Task Handle(UpdateAlertSubscriptionCommand request, CancellationToken cancellationToken)
    {
        await NotificationChannelEntitlements.RequireConfiguredChannelsAsync(featureFlags, request.Subscription.AccountId, [request.Subscription.Channel], cancellationToken);
        await writer.UpdateAlertSubscriptionAsync(request.AlertSubscriptionId, request.Subscription, cancellationToken);
    }
}
public class UpdateAlertSubscriptionCommandValidator : AbstractValidator<UpdateAlertSubscriptionCommand>
{
    public UpdateAlertSubscriptionCommandValidator()
    {
        AlertSubscriptionContractRules.Apply(this, x => x.Subscription);
    }
}

[Authorize(Resource = Resources.Notifications, Action = Actions.Delete)]
[RequireFeature(FeatureKeys.Notifications)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct DeleteAlertSubscriptionCommand(Guid AlertSubscriptionId) : IRequest<Guid>;
public class DeleteAlertSubscriptionCommandHandler(IAlertSubscriptionWriter writer) : IRequestHandler<DeleteAlertSubscriptionCommand, Guid>
{
    public async Task<Guid> Handle(DeleteAlertSubscriptionCommand request, CancellationToken cancellationToken)
    {
        await writer.DeleteAlertSubscriptionAsync(request.AlertSubscriptionId, cancellationToken);
        return request.AlertSubscriptionId;
    }
}
