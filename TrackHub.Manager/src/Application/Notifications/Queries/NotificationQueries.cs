namespace TrackHub.Manager.Application.Notifications.Queries;

[Authorize(Resource = Resources.Notifications, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct GetNotificationRulesQuery(Guid AccountId, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<NotificationRuleVm>>;
public class GetNotificationRulesQueryHandler(INotificationReader reader) : IRequestHandler<GetNotificationRulesQuery, IReadOnlyCollection<NotificationRuleVm>>
{
    public async Task<IReadOnlyCollection<NotificationRuleVm>> Handle(GetNotificationRulesQuery request, CancellationToken cancellationToken) => await reader.GetNotificationRulesAsync(request.AccountId, request.Skip, request.Take, cancellationToken);
}

[Authorize(Resource = Resources.Notifications, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct GetNotificationDeliveriesQuery(Guid AccountId, string? Status = null, string? Channel = null, DateTimeOffset? From = null, DateTimeOffset? To = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<NotificationDeliveryVm>>;
public class GetNotificationDeliveriesQueryHandler(INotificationReader reader) : IRequestHandler<GetNotificationDeliveriesQuery, IReadOnlyCollection<NotificationDeliveryVm>>
{
    public async Task<IReadOnlyCollection<NotificationDeliveryVm>> Handle(GetNotificationDeliveriesQuery request, CancellationToken cancellationToken)
        => await reader.GetNotificationDeliveriesAsync(request.AccountId, request.Status, request.Channel, request.From, request.To, request.Skip, request.Take, cancellationToken);
}

[Authorize(Resource = Resources.Notifications, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct GetAlertSubscriptionsQuery(Guid AccountId, Guid? PrincipalId = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<AlertSubscriptionVm>>;
public class GetAlertSubscriptionsQueryHandler(IAlertSubscriptionReader reader) : IRequestHandler<GetAlertSubscriptionsQuery, IReadOnlyCollection<AlertSubscriptionVm>>
{
    public async Task<IReadOnlyCollection<AlertSubscriptionVm>> Handle(GetAlertSubscriptionsQuery request, CancellationToken cancellationToken)
        => await reader.GetAlertSubscriptionsAsync(request.AccountId, request.PrincipalId, request.Skip, request.Take, cancellationToken);
}

[Authorize(Resource = Resources.Notifications, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct GetNotificationTemplatesQuery(Guid AccountId) : IRequest<IReadOnlyCollection<NotificationTemplateVm>>;
public class GetNotificationTemplatesQueryHandler(INotificationTemplateReader reader) : IRequestHandler<GetNotificationTemplatesQuery, IReadOnlyCollection<NotificationTemplateVm>>
{
    public async Task<IReadOnlyCollection<NotificationTemplateVm>> Handle(GetNotificationTemplatesQuery request, CancellationToken cancellationToken)
        => await reader.GetNotificationTemplatesAsync(request.AccountId, cancellationToken);
}

// The in-app feed is platform baseline — receiving is not feature-gated.
[Authorize(Resource = Resources.Notifications, Action = Actions.Read, PrincipalTypes = "User,Driver")]
public readonly record struct GetMyNotificationsQuery(bool UnreadOnly = false, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<MyNotificationVm>>;
public class GetMyNotificationsQueryHandler(INotificationReader reader) : IRequestHandler<GetMyNotificationsQuery, IReadOnlyCollection<MyNotificationVm>>
{
    public async Task<IReadOnlyCollection<MyNotificationVm>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
        => await reader.GetMyNotificationsAsync(request.UnreadOnly, request.Skip, request.Take, cancellationToken);
}

[Authorize(Resource = Resources.Notifications, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct GetDeliveryHealthQuery(Guid AccountId, DateTimeOffset From, DateTimeOffset To) : IRequest<IReadOnlyCollection<DeliveryHealthVm>>;
public class GetDeliveryHealthQueryHandler(INotificationReader reader) : IRequestHandler<GetDeliveryHealthQuery, IReadOnlyCollection<DeliveryHealthVm>>
{
    public async Task<IReadOnlyCollection<DeliveryHealthVm>> Handle(GetDeliveryHealthQuery request, CancellationToken cancellationToken)
        => await reader.GetDeliveryHealthAsync(request.AccountId, request.From, request.To, cancellationToken);
}
