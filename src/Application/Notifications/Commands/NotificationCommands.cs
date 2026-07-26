using Common.Application.Interfaces;

namespace TrackHub.Manager.Application.Notifications.Commands;

[Authorize(Resource = Resources.Notifications, Action = Actions.Write)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct CreateNotificationRuleCommand(NotificationRuleDto NotificationRule) : IRequest<NotificationRuleVm>;
public class CreateNotificationRuleCommandHandler(INotificationWriter writer, IFeatureFlagService featureFlags) : IRequestHandler<CreateNotificationRuleCommand, NotificationRuleVm>
{
    public async Task<NotificationRuleVm> Handle(CreateNotificationRuleCommand request, CancellationToken cancellationToken)
    {
        // Email/WhatsApp are per-account billable channels; configuring them requires the entitlement.
        await NotificationChannelEntitlements.RequireConfiguredChannelsAsync(featureFlags, request.NotificationRule.AccountId,
            NotificationRuleContracts.ParseChannels(request.NotificationRule.ChannelsJson), cancellationToken);
        return await writer.CreateNotificationRuleAsync(request.NotificationRule, cancellationToken);
    }
}
public class CreateNotificationRuleCommandValidator : AbstractValidator<CreateNotificationRuleCommand>
{
    public CreateNotificationRuleCommandValidator()
    {
        NotificationRuleContractRules.Apply(this, x => x.NotificationRule);
    }
}

[Authorize(Resource = Resources.Notifications, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct UpdateNotificationRuleCommand(Guid NotificationRuleId, NotificationRuleDto NotificationRule) : IRequest;
public class UpdateNotificationRuleCommandHandler(INotificationWriter writer, IFeatureFlagService featureFlags) : IRequestHandler<UpdateNotificationRuleCommand>
{
    public async Task Handle(UpdateNotificationRuleCommand request, CancellationToken cancellationToken)
    {
        await NotificationChannelEntitlements.RequireConfiguredChannelsAsync(featureFlags, request.NotificationRule.AccountId,
            NotificationRuleContracts.ParseChannels(request.NotificationRule.ChannelsJson), cancellationToken);
        await writer.UpdateNotificationRuleAsync(request.NotificationRuleId, request.NotificationRule, cancellationToken);
    }
}
public class UpdateNotificationRuleCommandValidator : AbstractValidator<UpdateNotificationRuleCommand>
{
    public UpdateNotificationRuleCommandValidator()
    {
        NotificationRuleContractRules.Apply(this, x => x.NotificationRule);
    }
}

[Authorize(Resource = Resources.Notifications, Action = Actions.Delete)]
[RequireFeature(FeatureKeys.Notifications)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct DisableNotificationRuleCommand(Guid NotificationRuleId) : IRequest;
public class DisableNotificationRuleCommandHandler(INotificationWriter writer) : IRequestHandler<DisableNotificationRuleCommand>
{
    public async Task Handle(DisableNotificationRuleCommand request, CancellationToken cancellationToken) => await writer.DisableNotificationRuleAsync(request.NotificationRuleId, cancellationToken);
}

[Authorize(Resource = Resources.Notifications, Action = Actions.Write)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct CreateNotificationDeliveryCommand(NotificationDeliveryDto NotificationDelivery) : IRequest<NotificationDeliveryVm>;
public class CreateNotificationDeliveryCommandHandler(INotificationWriter writer) : IRequestHandler<CreateNotificationDeliveryCommand, NotificationDeliveryVm>
{
    public async Task<NotificationDeliveryVm> Handle(CreateNotificationDeliveryCommand request, CancellationToken cancellationToken) => await writer.CreateNotificationDeliveryAsync(request.NotificationDelivery, cancellationToken);
}
