using TrackHub.Manager.Application.Notifications.Commands;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<NotificationRuleVm> CreateNotificationRule([Service] ISender sender, CreateNotificationRuleCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> UpdateNotificationRule([Service] ISender sender, UpdateNotificationRuleCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> DisableNotificationRule([Service] ISender sender, DisableNotificationRuleCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<NotificationDeliveryVm> CreateNotificationDelivery([Service] ISender sender, CreateNotificationDeliveryCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);

    public async Task<AlertSubscriptionVm> CreateAlertSubscription([Service] ISender sender, CreateAlertSubscriptionCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> UpdateAlertSubscription([Service] ISender sender, UpdateAlertSubscriptionCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<Guid> DeleteAlertSubscription([Service] ISender sender, DeleteAlertSubscriptionCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);

    public async Task<NotificationTemplateVm> CreateNotificationTemplate([Service] ISender sender, CreateNotificationTemplateCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> UpdateNotificationTemplate([Service] ISender sender, UpdateNotificationTemplateCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<Guid> DeleteNotificationTemplate([Service] ISender sender, DeleteNotificationTemplateCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);

    public async Task<bool> RetryNotificationDelivery([Service] ISender sender, RetryNotificationDeliveryCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> MarkNotificationRead([Service] ISender sender, MarkNotificationReadCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<NotificationDeliveryVm> SendTestNotification([Service] ISender sender, SendTestNotificationCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
}
