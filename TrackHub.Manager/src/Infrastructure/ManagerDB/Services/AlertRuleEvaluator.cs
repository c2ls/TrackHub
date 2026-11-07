using System.Text.Json;
using Common.Domain.Constants;
using Microsoft.Extensions.Logging;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Services;

/// <summary>
/// Rule evaluation for recorded alert events, run by the AlertRecorder after the row is saved;
/// per-rule failures are logged and never propagate. Channel entitlements are checked here (skip creating what could never send)
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

        if (await IsFailureOfAFailureAlertAsync(alertEvent, cancellationToken))
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

        // Self-service subscriptions fan out once per event, carried by the first rule that admits them.
        var subscriptionCarrier = rules
            .Where(AdmitsSubscribers)
            .OrderBy(r => r.RuleKey, StringComparer.Ordinal)
            .ThenBy(r => r.NotificationRuleId)
            .FirstOrDefault();

        var created = 0;
        foreach (var rule in rules)
        {
            try
            {
                created += await EvaluateRuleAsync(rule, alertEvent, emailEnabled, whatsAppEnabled, ReferenceEquals(rule, subscriptionCarrier), now, cancellationToken);
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

    private async Task<int> EvaluateRuleAsync(NotificationRule rule, AlertEventVm alertEvent, bool emailEnabled, bool whatsAppEnabled, bool includeSubscribers, DateTimeOffset now, CancellationToken cancellationToken)
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

        var subscriptions = includeSubscribers
            ? await SubscriptionsAllowedToSeeAsync(alertEvent, cancellationToken)
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

    // A rule whose selector does not parse fails on its own below; it must not take the event's other rules with it.
    private static bool AdmitsSubscribers(NotificationRule rule)
    {
        try
        {
            return NotificationRuleContracts.ParseRecipientSelector(rule.RecipientSelector).Subscribers ?? true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    // A delivery that fails while carrying a delivery-failure alert must not spawn a third alert.
    private async Task<bool> IsFailureOfAFailureAlertAsync(AlertEventVm alertEvent, CancellationToken cancellationToken)
        => alertEvent.EventType == AlertEventTypes.NotificationDeliveryFailed
            && Guid.TryParse(alertEvent.ResourceId, out var deliveryId)
            && await (from d in context.NotificationDeliveries
                      join a in context.AlertEvents on d.AlertEventId equals a.AlertEventId
                      where d.NotificationDeliveryId == deliveryId
                      select a.EventType)
                .AnyAsync(t => t == AlertEventTypes.NotificationDeliveryFailed, cancellationToken);

    // E-mail and WhatsApp are rendered here, so a subscriber's group scope has to be applied here too
    // (the in-app feed re-applies it at read time): users through the one visibility predicate,
    // staff through their default or currently assigned vehicle.
    private async Task<List<AlertSubscription>> SubscriptionsAllowedToSeeAsync(AlertEventVm alertEvent, CancellationToken cancellationToken)
    {
        var subscriptions = await context.AlertSubscriptions
            .Where(s => s.AccountId == alertEvent.AccountId && s.Enabled
                && (s.EventTypeFilter == null || s.EventTypeFilter == alertEvent.EventType))
            .ToListAsync(cancellationToken);

        if (subscriptions.Count == 0 || alertEvent.ResourceType != "Transporter" || !Guid.TryParse(alertEvent.ResourceId, out var transporterId))
        {
            return subscriptions;
        }

        var allowed = new List<AlertSubscription>(subscriptions.Count);
        foreach (var subscription in subscriptions)
        {
            var visible = subscription.PrincipalType switch
            {
                RecipientPrincipalTypes.User => await TransporterVisibility.IsPrivilegedAsync(context, subscription.PrincipalId, cancellationToken)
                    || await TransporterVisibility.Query(context, subscription.PrincipalId, alertEvent.AccountId, privileged: false)
                        .AnyAsync(t => t.TransporterId == transporterId, cancellationToken),
                RecipientPrincipalTypes.Staff => await StaffSeesAsync(subscription.PrincipalId, transporterId, cancellationToken),
                _ => false,
            };

            if (visible)
            {
                allowed.Add(subscription);
            }
        }

        return allowed;
    }

    private async Task<bool> StaffSeesAsync(Guid staffId, Guid transporterId, CancellationToken cancellationToken)
        => await context.StaffMembers.AnyAsync(d => d.StaffId == staffId && d.DefaultTransporterId == transporterId, cancellationToken)
            || await context.StaffTransporterAssignments.AnyAsync(a => a.StaffId == staffId && a.TransporterId == transporterId && a.Status == StaffAssignmentStatuses.Active, cancellationToken);

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

        if (selector.StaffIds is { Count: > 0 })
        {
            var staffIds = await context.StaffMembers
                .Where(d => d.AccountId == accountId && selector.StaffIds.Contains(d.StaffId))
                .Select(d => d.StaffId)
                .ToListAsync(cancellationToken);
            foreach (var staffId in staffIds)
            {
                recipients.Add((NotificationChannels.InApp, RecipientPrincipalTypes.Staff, staffId.ToString()));
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
