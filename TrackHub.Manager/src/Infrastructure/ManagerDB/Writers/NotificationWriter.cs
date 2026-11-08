using System.Text.Json;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Application.Notifications;
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
        AddAuditEvent(accountId, "CreateNotificationRule", "NotificationRule", $"{entity.NotificationRuleId}", null, Describe(entity));
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task UpdateNotificationRuleAsync(Guid notificationRuleId, NotificationRuleDto notificationRule, CancellationToken cancellationToken)
    {
        RequirePrivileged();
        var entity = await RequireScopedAsync(Context.NotificationRules.AsTracking(), x => x.NotificationRuleId == notificationRuleId, x => x.AccountId, notificationRuleId, forWrite: true, cancellationToken);
        RowVersion.Expect(Context.NotificationRules, entity, notificationRule.ExpectedVersion);
        if (notificationRule.AccountId != entity.AccountId)
        {
            throw new ForbiddenAccessException();
        }

        await RequireSelectorPrincipalsInAccountAsync(entity.AccountId, notificationRule.RecipientSelector, cancellationToken);
        var previous = Describe(entity);
        entity.RuleKey = notificationRule.RuleKey;
        entity.RuleType = notificationRule.RuleType;
        entity.Enabled = notificationRule.Enabled;
        entity.TriggerEvent = notificationRule.TriggerEvent;
        entity.RecipientSelector = notificationRule.RecipientSelector;
        entity.ChannelsJson = notificationRule.ChannelsJson;
        entity.ThrottlingJson = notificationRule.ThrottlingJson;
        entity.ConfigurationJson = Services.NotificationRuleConfigurationJson.PreserveSecret(notificationRule.ConfigurationJson, entity.ConfigurationJson);
        if (!NotificationRuleContractRules.HaveWebhookSecretWhenSelected(notificationRule with { ConfigurationJson = entity.ConfigurationJson }))
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure(nameof(notificationRule.ConfigurationJson), "Rules with the Webhook channel require configurationJson.webhookSecret.")]);
        }

        AddAuditEvent(entity.AccountId, "UpdateNotificationRule", "NotificationRule", $"{entity.NotificationRuleId}", previous, Describe(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DisableNotificationRuleAsync(Guid notificationRuleId, CancellationToken cancellationToken)
    {
        RequirePrivileged();
        var entity = await RequireScopedAsync(Context.NotificationRules.AsTracking(), x => x.NotificationRuleId == notificationRuleId, x => x.AccountId, notificationRuleId, forWrite: true, cancellationToken);
        entity.Enabled = false;
        AddAuditEvent(entity.AccountId, "DisableNotificationRule", "NotificationRule", $"{entity.NotificationRuleId}", null, Describe(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<NotificationDeliveryVm> CreateNotificationDeliveryAsync(NotificationDeliveryDto notificationDelivery, CancellationToken cancellationToken)
    {
        // A delivery row is an outbound send instruction — plain users must not mint them.
        RequirePrivileged();
        var accountId = RequireAccountWriteAccess(notificationDelivery.AccountId);
        if (notificationDelivery.NotificationRuleId is { } ruleId
            && !await Context.NotificationRules.AnyAsync(x => x.NotificationRuleId == ruleId && x.AccountId == accountId, cancellationToken))
        {
            throw new NotFoundException(nameof(NotificationRule), ruleId.ToString());
        }

        if (notificationDelivery.AlertEventId is { } alertEventId
            && !await Context.AlertEvents.AnyAsync(x => x.AlertEventId == alertEventId && x.AccountId == accountId, cancellationToken))
        {
            throw new NotFoundException(nameof(AlertEvent), alertEventId.ToString());
        }

        var entity = new NotificationDelivery(accountId, notificationDelivery.NotificationRuleId, notificationDelivery.AlertEventId, notificationDelivery.Channel, notificationDelivery.RecipientPrincipalType, notificationDelivery.Recipient, notificationDelivery.Status);
        await Context.NotificationDeliveries.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return new NotificationDeliveryVm(entity.NotificationDeliveryId, entity.AccountId, entity.NotificationRuleId, entity.AlertEventId, entity.Channel, entity.RecipientPrincipalType, entity.Recipient, entity.Status, entity.Attempts, entity.ProviderMessageId, entity.Error, entity.SentAt, entity.ReadAt, entity.LastModified);
    }

    public async Task RetryNotificationDeliveryAsync(Guid notificationDeliveryId, CancellationToken cancellationToken)
    {
        RequirePrivileged();
        var entity = await RequireScopedAsync(Context.NotificationDeliveries.AsTracking(), x => x.NotificationDeliveryId == notificationDeliveryId, x => x.AccountId, notificationDeliveryId, forWrite: true, cancellationToken);
        if (entity.Status != DeliveryStatuses.Failed)
        {
            throw new ConflictException("Only Failed deliveries can be retried.");
        }

        // Attempt counter preserved on manual retry.
        entity.Status = DeliveryStatuses.Pending;
        entity.Error = null;
        AddAuditEvent(entity.AccountId, "RetryNotificationDelivery", "NotificationDelivery", $"{entity.NotificationDeliveryId}", null,
            $$"""{"channel":{{AuditJson.Quote(entity.Channel)}},"attempts":{{entity.Attempts}}}""");
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkNotificationReadAsync(Guid notificationDeliveryId, CancellationToken cancellationToken)
    {
        var entity = await Context.NotificationDeliveries
            .AsTracking().FirstOrDefaultAsync(x => x.NotificationDeliveryId == notificationDeliveryId, cancellationToken);
        if (entity is null || !IsRecipient(entity))
        {
            throw new NotFoundException(nameof(NotificationDelivery), notificationDeliveryId.ToString());
        }

        if (entity.ReadAt == null)
        {
            entity.ReadAt = DateTimeOffset.UtcNow;
            await Context.SaveChangesAsync(cancellationToken);
        }
    }


    // Cross-account recipients are invalid: every explicit user/staff id in the
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

        if (selector.StaffIds is { Count: > 0 })
        {
            var staffIds = selector.StaffIds.Distinct().ToList();
            var known = await Context.StaffMembers.CountAsync(d => d.AccountId == accountId && staffIds.Contains(d.StaffId), cancellationToken);
            if (known != staffIds.Count)
            {
                throw new ForbiddenAccessException("recipientSelector.staffIds contains principals outside the account.");
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

        return Principal.PrincipalType == PrincipalType.Staff
            && Principal.StaffId.HasValue
            && entity.RecipientPrincipalType == RecipientPrincipalTypes.Staff
            && entity.Recipient == Principal.StaffId.Value.ToString();
    }

    private static NotificationRuleVm ToVm(NotificationRule x) => new(x.NotificationRuleId, x.AccountId, x.RuleKey, x.RuleType, x.Enabled, x.TriggerEvent, x.RecipientSelector, x.ChannelsJson, x.ThrottlingJson, x.ConfigurationJson, x.LastModified, x.Version);
    private static string Describe(NotificationRule rule)
        => $$"""{"ruleKey":{{AuditJson.Quote(rule.RuleKey)}},"ruleType":{{AuditJson.Quote(rule.RuleType)}},"enabled":{{rule.Enabled.ToString().ToLowerInvariant()}},"triggerEvent":{{AuditJson.Quote(rule.TriggerEvent)}},"recipientSelector":{{AuditJson.Quote(rule.RecipientSelector)}}}""";
}
