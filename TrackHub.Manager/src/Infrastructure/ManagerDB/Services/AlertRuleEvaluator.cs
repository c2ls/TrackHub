using Common.Domain.Constants;
using Microsoft.Extensions.Logging;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Services;

/// <summary>
/// Rule evaluation for recorded alert events. Runs post-commit inside the
/// recordAlertEvent pipeline via the AlertEventRecorded notification; per-rule failures are logged
/// and never propagate. Channel entitlements are checked here (skip creating what could never send)
/// and re-checked at dispatch time.
/// </summary>
public sealed class AlertRuleEvaluator(IApplicationDbContext context, ILogger<AlertRuleEvaluator> logger) : IAlertRuleEvaluator
{
    // Repeated detections coalesce into the same open AlertEvent (writer dedup); without an explicit
    // throttling contract the evaluator suppresses re-notification for the same rule+event for this
    // window. Rules opt out with dedupeWindowMinutes: 0.
    private const int DefaultDedupeWindowMinutes = 60;

    public async Task<int> EvaluateAsync(AlertEventVm alertEvent, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        if (!await FeatureEnabledAsync(alertEvent.AccountId, FeatureKeys.Notifications, now, cancellationToken))
        {
            return 0;
        }

        // Suspended accounts still record baseline alert events but get no notification fan-out
        //. A missing account row (in-memory tests) is not treated as suspended.
        if (await context.Accounts.AnyAsync(a => a.AccountId == alertEvent.AccountId && !a.Active, cancellationToken))
        {
            return 0;
        }

        var rules = await context.NotificationRules
            .Where(r => r.AccountId == alertEvent.AccountId && r.Enabled && r.TriggerEvent == alertEvent.EventType)
            .ToListAsync(cancellationToken);
        if (rules.Count == 0)
        {
            return 0;
        }

        var emailEnabled = await FeatureEnabledAsync(alertEvent.AccountId, FeatureKeys.NotificationsEmail, now, cancellationToken);
        var whatsAppEnabled = await FeatureEnabledAsync(alertEvent.AccountId, FeatureKeys.NotificationsWhatsApp, now, cancellationToken);

        var created = 0;
        foreach (var rule in rules)
        {
            try
            {
                created += await EvaluateRuleAsync(rule, alertEvent, emailEnabled, whatsAppEnabled, now, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Evaluation of rule {RuleKey} failed for alert event {AlertEventId}.", rule.RuleKey, alertEvent.AlertEventId);
            }
        }

        if (created > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return created;
    }

    private async Task<int> EvaluateRuleAsync(NotificationRule rule, AlertEventVm alertEvent, bool emailEnabled, bool whatsAppEnabled, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var throttling = NotificationRuleContracts.ParseThrottling(rule.ThrottlingJson);
        var dedupeWindowMinutes = throttling.DedupeWindowMinutes ?? DefaultDedupeWindowMinutes;
        if (dedupeWindowMinutes > 0)
        {
            var windowStart = now.AddMinutes(-dedupeWindowMinutes);
            var alreadyNotified = await context.NotificationDeliveries.AnyAsync(d =>
                d.NotificationRuleId == rule.NotificationRuleId
                && d.AlertEventId == alertEvent.AlertEventId
                && d.Created >= windowStart, cancellationToken);
            if (alreadyNotified)
            {
                return 0;
            }
        }

        if (throttling.MaxPerHour is > 0)
        {
            var hourStart = now.AddHours(-1);
            var recent = await context.NotificationDeliveries.CountAsync(d =>
                d.NotificationRuleId == rule.NotificationRuleId && d.Created >= hourStart, cancellationToken);
            if (recent >= throttling.MaxPerHour.Value)
            {
                return 0;
            }
        }

        var status = throttling.Digest is DigestCadences.Hourly or DigestCadences.Daily
            ? DeliveryStatuses.Deferred
            : DeliveryStatuses.Pending;

        var channels = NotificationRuleContracts.ParseChannels(rule.ChannelsJson).Distinct().ToList();
        var selector = NotificationRuleContracts.ParseRecipientSelector(rule.RecipientSelector);
        var configuration = NotificationRuleContracts.ParseConfiguration(rule.ConfigurationJson);

        // Self-service subscriptions fan out unless the rule explicitly opts out with subscribers: false.
        var includeSubscribers = selector.Subscribers ?? true;
        var subscriptions = includeSubscribers
            ? await context.AlertSubscriptions
                .Where(s => s.AccountId == alertEvent.AccountId && s.Enabled
                    && (s.EventTypeFilter == null || s.EventTypeFilter == alertEvent.EventType))
                .ToListAsync(cancellationToken)
            : [];

        var recipients = new HashSet<(string Channel, string PrincipalType, string Recipient)>();

        foreach (var channel in channels)
        {
            switch (channel)
            {
                case NotificationChannels.InApp:
                    await ResolveInAppRecipientsAsync(alertEvent.AccountId, selector, recipients, cancellationToken);
                    break;
                case NotificationChannels.Email when emailEnabled:
                    ResolveContactRecipients(selector, NotificationChannels.Email, NotificationRuleContracts.IsEmail, recipients);
                    break;
                case NotificationChannels.WhatsApp when whatsAppEnabled:
                    ResolveContactRecipients(selector, NotificationChannels.WhatsApp, NotificationRuleContracts.IsE164, recipients);
                    break;
                case NotificationChannels.Email:
                case NotificationChannels.WhatsApp:
                    logger.LogInformation("Skipping {Channel} for rule {RuleKey}: channel entitlement disabled for account {AccountId}.", channel, rule.RuleKey, alertEvent.AccountId);
                    break;
                case NotificationChannels.Webhook when !string.IsNullOrWhiteSpace(configuration.WebhookUrl):
                    recipients.Add((NotificationChannels.Webhook, RecipientPrincipalTypes.Rule, configuration.WebhookUrl!));
                    break;
                case NotificationChannels.Push:
                    // Provider contract only; the push implementation is not yet available.
                    break;
            }
        }

        // Subscriptions carry their own channel; entitlements still apply per channel.
        foreach (var subscription in subscriptions)
        {
            switch (subscription.Channel)
            {
                case NotificationChannels.InApp:
                    recipients.Add((NotificationChannels.InApp, subscription.PrincipalType, subscription.PrincipalId.ToString()));
                    break;
                case NotificationChannels.Email when emailEnabled && NotificationRuleContracts.IsEmail(subscription.Contact):
                    recipients.Add((NotificationChannels.Email, subscription.PrincipalType, subscription.Contact!));
                    break;
                case NotificationChannels.WhatsApp when whatsAppEnabled && NotificationRuleContracts.IsE164(subscription.Contact):
                    recipients.Add((NotificationChannels.WhatsApp, subscription.PrincipalType, subscription.Contact!));
                    break;
            }
        }

        foreach (var (channel, principalType, recipient) in recipients)
        {
            // Created is stamped here so the dedupe-window query works even where the auditable
            // interceptor is absent (in-memory tests); the interceptor writes the same instant.
            await context.NotificationDeliveries.AddAsync(
                new NotificationDelivery(alertEvent.AccountId, rule.NotificationRuleId, alertEvent.AlertEventId, channel, principalType, recipient, status) { Created = now },
                cancellationToken);
        }

        return recipients.Count;
    }

    private async Task ResolveInAppRecipientsAsync(Guid accountId, RecipientSelectorModel selector, HashSet<(string, string, string)> recipients, CancellationToken cancellationToken)
    {
        if (selector.UserIds is { Count: > 0 })
        {
            // Account membership is re-checked at evaluation time; stale selector ids are dropped.
            var userIds = await context.Users
                .Where(u => u.AccountId == accountId && selector.UserIds.Contains(u.UserId))
                .Select(u => u.UserId)
                .ToListAsync(cancellationToken);
            foreach (var userId in userIds)
            {
                recipients.Add((NotificationChannels.InApp, RecipientPrincipalTypes.User, userId.ToString()));
            }
        }

        if (selector.DriverIds is { Count: > 0 })
        {
            var driverIds = await context.Drivers
                .Where(d => d.AccountId == accountId && selector.DriverIds.Contains(d.DriverId))
                .Select(d => d.DriverId)
                .ToListAsync(cancellationToken);
            foreach (var driverId in driverIds)
            {
                recipients.Add((NotificationChannels.InApp, RecipientPrincipalTypes.Driver, driverId.ToString()));
            }
        }

        // Role recipients are a single role-addressed row; the in-app feed fans out at read time.
        foreach (var role in selector.Roles ?? [])
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                recipients.Add((NotificationChannels.InApp, RecipientPrincipalTypes.Role, role));
            }
        }
    }

    private static void ResolveContactRecipients(RecipientSelectorModel selector, string channel, Func<string?, bool> isValidAddress, HashSet<(string, string, string)> recipients)
    {
        foreach (var contact in selector.Contacts ?? [])
        {
            if (string.Equals(contact.Channel, channel, StringComparison.OrdinalIgnoreCase) && isValidAddress(contact.Address))
            {
                recipients.Add((channel, RecipientPrincipalTypes.Contact, contact.Address));
            }
        }
    }

    private async Task<bool> FeatureEnabledAsync(Guid accountId, string featureKey, DateTimeOffset now, CancellationToken cancellationToken)
        => await context.AccountFeatures.AnyAsync(f =>
            f.AccountId == accountId && f.FeatureKey == featureKey && f.Enabled
            && (f.EffectiveFrom == null || f.EffectiveFrom <= now)
            && (f.EffectiveTo == null || f.EffectiveTo >= now), cancellationToken);
}
