using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Application.Notifications.Commands;

[Authorize(Resource = Resources.Notifications, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.Notifications)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct RetryNotificationDeliveryCommand(Guid NotificationDeliveryId) : IRequest;
public class RetryNotificationDeliveryCommandHandler(INotificationWriter writer) : IRequestHandler<RetryNotificationDeliveryCommand>
{
    public async Task Handle(RetryNotificationDeliveryCommand request, CancellationToken cancellationToken) => await writer.RetryNotificationDeliveryAsync(request.NotificationDeliveryId, cancellationToken);
}

// Receiving in-app notifications is not feature-gated: mark-read stays ungated.
[Authorize(Resource = Resources.Notifications, Action = Actions.Edit, PrincipalTypes = "User,Driver")]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct MarkNotificationReadCommand(Guid NotificationDeliveryId) : IRequest;
public class MarkNotificationReadCommandHandler(INotificationWriter writer) : IRequestHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken) => await writer.MarkNotificationReadAsync(request.NotificationDeliveryId, cancellationToken);
}

/// <summary>
/// Queues one test delivery through the normal dispatch pipeline; the dispatcher
/// renders the built-in TestNotification template. Backs the portal channel-setup UI.
/// </summary>
[Authorize(Resource = Resources.Notifications, Action = Actions.Custom)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct SendTestNotificationCommand(Guid AccountId, string Channel, string? Contact) : IRequest<NotificationDeliveryVm>;
public class SendTestNotificationCommandHandler(INotificationWriter writer, IFeatureFlagService featureFlags, ICurrentPrincipal principal) : IRequestHandler<SendTestNotificationCommand, NotificationDeliveryVm>
{
    public async Task<NotificationDeliveryVm> Handle(SendTestNotificationCommand request, CancellationToken cancellationToken)
    {
        await NotificationChannelEntitlements.RequireConfiguredChannelsAsync(featureFlags, request.AccountId, [request.Channel], cancellationToken);

        var (principalType, recipient) = request.Channel == NotificationChannels.InApp
            ? (RecipientPrincipalTypes.User, (principal.UserId
                ?? throw new ForbiddenAccessException("In-app test notifications require a user principal.")).ToString())
            : (RecipientPrincipalTypes.Contact, request.Contact!);

        var delivery = new NotificationDeliveryDto(request.AccountId, null, null, request.Channel, principalType, recipient, DeliveryStatuses.Pending);
        return await writer.CreateNotificationDeliveryAsync(delivery, cancellationToken);
    }
}
public class SendTestNotificationCommandValidator : AbstractValidator<SendTestNotificationCommand>
{
    public SendTestNotificationCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Channel)
            .Must(c => c is NotificationChannels.InApp or NotificationChannels.Email or NotificationChannels.WhatsApp)
            .WithMessage("Test notifications support the InApp, Email, and WhatsApp channels.");
        RuleFor(x => x)
            .Must(x => x.Channel switch
            {
                NotificationChannels.Email => NotificationRuleContracts.IsEmail(x.Contact),
                NotificationChannels.WhatsApp => NotificationRuleContracts.IsE164(x.Contact),
                _ => true
            })
            .OverridePropertyName("Contact")
            .WithMessage("Email tests require a valid email contact; WhatsApp tests require an E.164 phone contact.");
    }
}
