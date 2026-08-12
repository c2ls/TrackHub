using System.Text.Json;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class NotificationWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), INotificationWriter
{
    public async Task<NotificationRuleVm> CreateNotificationRuleAsync(NotificationRuleDto notificationRule, CancellationToken cancellationToken)
    {
        RequirePrivileged();
        var accountId = RequireAccountWriteAccess(notificationRule.AccountId);
        await RequireSelectorPrincipalsInAccountAsync(accountId, notificationRule.RecipientSelector, cancellationToken);
        var entity = new NotificationRule(accountId, notificationRule.RuleKey, notificationRule.RuleType, notificationRule.Enabled, notificationRule.TriggerEvent, notificationRule.RecipientSelector, notificationRule.ChannelsJson, notificationRule.ThrottlingJson, notificationRule.ConfigurationJson);
        await Context.NotificationRules.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task UpdateNotificationRuleAsync(Guid notificationRuleId, NotificationRuleDto notificationRule, CancellationToken cancellationToken)
    {
        RequirePrivileged();
        var entity = await Context.NotificationRules.FirstAsync(x => x.NotificationRuleId == notificationRuleId, cancellationToken);
        RequireAccountWriteAccess(entity.AccountId);
        if (notificationRule.AccountId != entity.AccountId)
        {
            throw new ForbiddenAccessException();
        }

        await RequireSelectorPrincipalsInAccountAsync(entity.AccountId, notificationRule.RecipientSelector, cancellationToken);
        Context.NotificationRules.Attach(entity);
        entity.RuleKey = notificationRule.RuleKey;
        entity.RuleType = notificationRule.RuleType;
        entity.Enabled = notificationRule.Enabled;
        entity.TriggerEvent = notificationRule.TriggerEvent;
        entity.RecipientSelector = notificationRule.RecipientSelector;
        entity.ChannelsJson = notificationRule.ChannelsJson;
        entity.ThrottlingJson = notificationRule.ThrottlingJson;
        entity.ConfigurationJson = notificationRule.ConfigurationJson;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DisableNotificationRuleAsync(Guid notificationRuleId, CancellationToken cancellationToken)
    {
        RequirePrivileged();
        var entity = await Context.NotificationRules.FirstAsync(x => x.NotificationRuleId == notificationRuleId, cancellationToken);
        RequireAccountWriteAccess(entity.AccountId);
        Context.NotificationRules.Attach(entity);
        entity.Enabled = false;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<NotificationDeliveryVm> CreateNotificationDeliveryAsync(NotificationDeliveryDto notificationDelivery, CancellationToken cancellationToken)
    {
        // A delivery row is an outbound send instruction — plain users must not mint them.
        RequirePrivileged();
        var entity = new NotificationDelivery(RequireAccountWriteAccess(notificationDelivery.AccountId), notificationDelivery.NotificationRuleId, notificationDelivery.AlertEventId, notificationDelivery.Channel, notificationDelivery.RecipientPrincipalType, notificationDelivery.Recipient, notificationDelivery.Status);
        await Context.NotificationDeliveries.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return new NotificationDeliveryVm(entity.NotificationDeliveryId, entity.AccountId, entity.NotificationRuleId, entity.AlertEventId, entity.Channel, entity.RecipientPrincipalType, entity.Recipient, entity.Status, entity.Attempts, entity.ProviderMessageId, entity.Error, entity.SentAt, entity.ReadAt, entity.LastModified);
    }

    public async Task RetryNotificationDeliveryAsync(Guid notificationDeliveryId, CancellationToken cancellationToken)
    {
        RequirePrivileged();
        var entity = await Context.NotificationDeliveries.FirstAsync(x => x.NotificationDeliveryId == notificationDeliveryId, cancellationToken);
        RequireAccountWriteAccess(entity.AccountId);
        if (entity.Status != DeliveryStatuses.Failed)
        {
            throw new ConflictException("Only Failed deliveries can be retried.");
        }

        Context.NotificationDeliveries.Attach(entity);
        // Attempt counter preserved on manual retry.
        entity.Status = DeliveryStatuses.Pending;
        entity.Error = null;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkNotificationReadAsync(Guid notificationDeliveryId, CancellationToken cancellationToken)
    {
        var entity = await Context.NotificationDeliveries.FirstAsync(x => x.NotificationDeliveryId == notificationDeliveryId, cancellationToken);
        if (!IsRecipient(entity))
        {
            throw new ForbiddenAccessException("Only the recipient may mark a notification read.");
        }

        if (entity.ReadAt == null)
        {
            Context.NotificationDeliveries.Attach(entity);
            entity.ReadAt = DateTimeOffset.UtcNow;
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    // Rules, deliveries, and retries are administrative surfaces: the Notifications
    // resource-action grants are held by every portal role for the self-service surfaces
    // (feed/mark-read/subscriptions), so the admin-only distinction is enforced here.
    private void RequirePrivileged()
    {
        if (!IsPrivileged)
        {
            throw new ForbiddenAccessException("Only administrators or managers may manage notification rules and deliveries.");
        }
    }

    // Cross-account recipients are invalid: every explicit user/driver id in the
    // selector must belong to the rule's account. Malformed JSON is rejected upstream by the
    // command validator; this guard owns the membership part.
    private async Task RequireSelectorPrincipalsInAccountAsync(Guid accountId, string recipientSelector, CancellationToken cancellationToken)
    {
        RecipientSelectorModel selector;
        try
        {
            selector = NotificationRuleContracts.ParseRecipientSelector(recipientSelector);
        }
        catch (JsonException)
        {
            return;
        }

        if (selector.UserIds is { Count: > 0 })
        {
            var userIds = selector.UserIds.Distinct().ToList();
            var known = await Context.Users.CountAsync(u => u.AccountId == accountId && userIds.Contains(u.UserId), cancellationToken);
            if (known != userIds.Count)
            {
                throw new ForbiddenAccessException("recipientSelector.userIds contains principals outside the account.");
            }
        }

        if (selector.DriverIds is { Count: > 0 })
        {
            var driverIds = selector.DriverIds.Distinct().ToList();
            var known = await Context.Drivers.CountAsync(d => d.AccountId == accountId && driverIds.Contains(d.DriverId), cancellationToken);
            if (known != driverIds.Count)
            {
                throw new ForbiddenAccessException("recipientSelector.driverIds contains principals outside the account.");
            }
        }
    }

    private bool IsRecipient(NotificationDelivery entity)
    {
        if (Principal.PrincipalType == PrincipalType.User && Principal.UserId.HasValue)
        {
            if (entity.RecipientPrincipalType == RecipientPrincipalTypes.User)
            {
                return entity.Recipient == Principal.UserId.Value.ToString();
            }

            // Role-addressed rows are shared by every account user holding the role; any of them may
            // mark the single delivery read.
            if (entity.RecipientPrincipalType == RecipientPrincipalTypes.Role)
            {
                return string.Equals(entity.Recipient, Principal.Role, StringComparison.OrdinalIgnoreCase)
                    && Context.Users.Any(u => u.UserId == Principal.UserId.Value && u.AccountId == entity.AccountId);
            }

            return false;
        }

        return Principal.PrincipalType == PrincipalType.Driver
            && Principal.DriverId.HasValue
            && entity.RecipientPrincipalType == RecipientPrincipalTypes.Driver
            && entity.Recipient == Principal.DriverId.Value.ToString();
    }

    private static NotificationRuleVm ToVm(NotificationRule x) => new(x.NotificationRuleId, x.AccountId, x.RuleKey, x.RuleType, x.Enabled, x.TriggerEvent, x.RecipientSelector, x.ChannelsJson, x.ThrottlingJson, x.ConfigurationJson, x.LastModified);
}
