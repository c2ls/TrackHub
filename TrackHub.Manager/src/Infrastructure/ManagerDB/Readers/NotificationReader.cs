using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class NotificationReader(IApplicationDbContext context, ICurrentPrincipal principal, IVisibleTransporterReader visibleTransporters) : AccountScopedDataAccess(context, principal), INotificationReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<IReadOnlyCollection<NotificationRuleVm>> GetNotificationRulesAsync(Guid accountId, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        return await Context.NotificationRules
            .Where(x => x.AccountId == scopedAccountId)
            .OrderBy(x => x.RuleKey).ThenBy(x => x.NotificationRuleId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(x => new NotificationRuleVm(x.NotificationRuleId, x.AccountId, x.RuleKey, x.RuleType, x.Enabled, x.TriggerEvent, x.RecipientSelector, x.ChannelsJson, x.ThrottlingJson, x.ConfigurationJson, x.LastModified))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<NotificationDeliveryVm>> GetNotificationDeliveriesAsync(Guid accountId, string? status, string? channel, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        var query = Context.NotificationDeliveries.Where(x => x.AccountId == scopedAccountId);
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(x => x.Status == status);
        }
        if (!string.IsNullOrEmpty(channel))
        {
            query = query.Where(x => x.Channel == channel);
        }
        if (from.HasValue)
        {
            query = query.Where(x => x.Created >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(x => x.Created <= to.Value);
        }

        var rows = await query
            .OrderByDescending(x => x.Created).ThenBy(x => x.NotificationDeliveryId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .ToListAsync(cancellationToken);

        // Contact endpoints are personal data: list VMs mask them.
        return rows
            .Select(x => new NotificationDeliveryVm(x.NotificationDeliveryId, x.AccountId, x.NotificationRuleId, x.AlertEventId, x.Channel, x.RecipientPrincipalType, MaskRecipient(x.Channel, x.Recipient), x.Status, x.Attempts, x.ProviderMessageId, x.Error, x.SentAt, x.ReadAt, x.LastModified))
            .ToList();
    }

    public async Task<IReadOnlyCollection<MyNotificationVm>> GetMyNotificationsAsync(bool unreadOnly, int skip, int take, CancellationToken cancellationToken)
    {
        var query = Context.NotificationDeliveries.Where(x => x.Channel == NotificationChannels.InApp
            && (x.Status == DeliveryStatuses.Sent || x.Status == DeliveryStatuses.Pending || x.Status == DeliveryStatuses.Sending));

        IReadOnlyCollection<string>? visibleTransporterKeys = null;
        if (Principal.PrincipalType == PrincipalType.User && Principal.UserId.HasValue)
        {
            var userKey = Principal.UserId.Value.ToString();
            var role = Principal.Role ?? string.Empty;
            var accountId = ResolveAccountScope(null)
                ?? throw new ForbiddenAccessException("Insufficient permissions. Required account access: current principal must resolve an account id.");
            // Role-addressed deliveries fan out at read time: any user holding the role in the
            // account sees the (single) delivery row.
            query = query.Where(x =>
                (x.RecipientPrincipalType == RecipientPrincipalTypes.User && x.Recipient == userKey)
                || (x.RecipientPrincipalType == RecipientPrincipalTypes.Role && x.Recipient == role && x.AccountId == accountId));

            // Group visibility follows the source resource: non-privileged users
            // never see notifications for transporters outside their groups. The subscriber's role
            // is unknown at evaluation time, so the filter is applied here where the token is.
            if (!IsPrivileged)
            {
                var visibleIds = await visibleTransporters.GetVisibleTransporterIdsAsync(Principal.UserId.Value, accountId, cancellationToken);
                visibleTransporterKeys = visibleIds.Select(id => id.ToString()).ToList();
            }
        }
        else if (Principal.PrincipalType == PrincipalType.Driver && Principal.DriverId.HasValue)
        {
            var driverKey = Principal.DriverId.Value.ToString();
            query = query.Where(x => x.RecipientPrincipalType == RecipientPrincipalTypes.Driver && x.Recipient == driverKey);
        }
        else
        {
            throw new ForbiddenAccessException("Only user or driver principals have an in-app notification feed.");
        }

        if (unreadOnly)
        {
            query = query.Where(x => x.ReadAt == null);
        }

        var joined = from d in query
                     join e in Context.AlertEvents on d.AlertEventId equals (Guid?)e.AlertEventId into events
                     from e in events.DefaultIfEmpty()
                     select new { Delivery = d, Event = e };

        if (visibleTransporterKeys != null)
        {
            joined = joined.Where(x => x.Event == null
                || x.Event.ResourceType != "Transporter"
                || visibleTransporterKeys.Contains(x.Event.ResourceId));
        }

        return await joined
            .OrderByDescending(x => x.Delivery.Created).ThenBy(x => x.Delivery.NotificationDeliveryId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(x => new MyNotificationVm(
                x.Delivery.NotificationDeliveryId,
                x.Delivery.AlertEventId,
                x.Event != null ? x.Event.EventType : null,
                x.Event != null ? x.Event.Severity : null,
                x.Event != null ? x.Event.SourceModule : null,
                x.Event != null ? x.Event.ResourceType : null,
                x.Event != null ? x.Event.ResourceId : null,
                x.Event != null ? x.Event.PayloadJson : null,
                x.Delivery.Created,
                x.Delivery.ReadAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DeliveryHealthVm>> GetDeliveryHealthAsync(Guid accountId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        return await Context.NotificationDeliveries
            .Where(x => x.AccountId == scopedAccountId && x.Created >= from && x.Created <= to)
            .GroupBy(x => new { x.Channel, x.Status })
            .Select(g => new DeliveryHealthVm(g.Key.Channel, g.Key.Status, g.Count(), g.Average(x => (double)x.Attempts)))
            .ToListAsync(cancellationToken);
    }

    private static string MaskRecipient(string channel, string recipient)
        => !NotificationChannels.RecipientIsContact(channel) || recipient.Length == 0
            ? recipient
            : recipient.Length <= 4 ? "****" : $"***{recipient[^4..]}";
}
