namespace TrackHub.Manager.Application.Notifications.Queries;

[Authorize(Resource = Resources.Notifications, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct GetNotificationRulesQuery(Guid AccountId, int Skip = 0, int Take = 50) : IRequest<NotificationRulesPageVm>;
public class GetNotificationRulesQueryHandler(INotificationReader reader) : IRequestHandler<GetNotificationRulesQuery, NotificationRulesPageVm>
{
    public async Task<NotificationRulesPageVm> Handle(GetNotificationRulesQuery request, CancellationToken cancellationToken) => await reader.GetNotificationRulesAsync(request.AccountId, request.Skip, request.Take, cancellationToken);
}

[Authorize(Resource = Resources.Notifications, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct GetNotificationDeliveriesQuery(Guid AccountId, string? Status = null, string? Channel = null, DateTimeOffset? From = null, DateTimeOffset? To = null, int Skip = 0, int Take = 50) : IRequest<NotificationDeliveriesPageVm>;
public class GetNotificationDeliveriesQueryHandler(INotificationReader reader) : IRequestHandler<GetNotificationDeliveriesQuery, NotificationDeliveriesPageVm>
{
    public async Task<NotificationDeliveriesPageVm> Handle(GetNotificationDeliveriesQuery request, CancellationToken cancellationToken)
        => await reader.GetNotificationDeliveriesAsync(request.AccountId, request.Status, request.Channel, request.From, request.To, request.Skip, request.Take, cancellationToken);
}

[Authorize(Resource = Resources.Notifications, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Notifications)]
// Enforcement: the writer or reader checks every referenced id against the request's account.
[AccountScopeEnforcedInHandler]
public readonly record struct GetAlertSubscriptionsQuery(Guid AccountId, Guid? PrincipalId = null, int Skip = 0, int Take = 50) : IRequest<AlertSubscriptionsPageVm>;
public class GetAlertSubscriptionsQueryHandler(IAlertSubscriptionReader reader) : IRequestHandler<GetAlertSubscriptionsQuery, AlertSubscriptionsPageVm>
{
    public async Task<AlertSubscriptionsPageVm> Handle(GetAlertSubscriptionsQuery request, CancellationToken cancellationToken)
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
[Authorize(Resource = Resources.Notifications, Action = Actions.Read, PrincipalTypes = "User,Staff")]
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
