// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Text.Json;
using Common.Domain.Constants;
using Common.Mediator;
using Microsoft.EntityFrameworkCore;
using TrackHub.Manager.Application.Notifications.Events;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.ManagerDB.Notifications;

namespace TrackHub.Manager.Web.BackgroundServices;

// Delivery dispatcher: scans Pending deliveries every 30 s and dispatches
// through the INotificationChannelProvider implementations. Retries with exponential backoff
// (1/5/15/60 min, max 5 attempts), then marks Failed with the provider error and raises a
// NotificationDeliveryFailed alert event. A Sending in-flight status prevents double-send on
// overlapping cycles; the delivery row itself is the idempotency record, and a row left in Sending
// by a crashed cycle is reclaimed after AppSettings:NotificationSendingReclaimMinutes. Channel entitlements are
// re-checked here: deliveries on a disabled billable channel are held (left Pending) so disabling
// the feature stops sends immediately without deleting configuration.
public sealed class NotificationDispatchService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<NotificationDispatchService> logger) : BackgroundService
{
    private const string JobKey = BackgroundJobKeys.NotificationDispatch;
    private const int MaxAttempts = 5;
    private const int BatchSize = 100;
    private const int DefaultSendingReclaimMinutes = 10;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await RunOnceAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "Notification dispatch cycle failed."); }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        var providers = scope.ServiceProvider.GetServices<INotificationChannelProvider>()
            .ToDictionary(p => p.Channel, StringComparer.Ordinal);

        var now = DateTimeOffset.UtcNow;

        await ReclaimStrandedAsync(context, now, cancellationToken);

        // Backoff eligibility is derived from LastModified (updated on every save): a delivery that
        // failed attempt N waits backoff(N) before the next try. Manual retries clear Error and are
        // picked up immediately.
        var eligible = await context.NotificationDeliveries
            .Where(d => d.Status == DeliveryStatuses.Pending
                && (d.Attempts == 0
                    || d.Error == null
                    || (d.Attempts == 1 && d.LastModified <= now.AddMinutes(-1))
                    || (d.Attempts == 2 && d.LastModified <= now.AddMinutes(-5))
                    || (d.Attempts == 3 && d.LastModified <= now.AddMinutes(-15))
                    || (d.Attempts >= 4 && d.LastModified <= now.AddMinutes(-60))))
            .OrderBy(d => d.Created)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (eligible.Count == 0)
        {
            return;
        }

        // Dispatch-time entitlement re-check: the base `notifications`
        // feature gates ALL dispatch for the account; Email/WhatsApp additionally need their
        // billable keys. Held deliveries stay Pending and resume when the feature is re-enabled.
        var accountIds = eligible.Select(d => d.AccountId).Distinct().ToList();
        var notificationsAccounts = await EnabledAccountsAsync(context, accountIds, FeatureKeys.Notifications, now, cancellationToken);
        var emailAccounts = await EnabledAccountsAsync(context, accountIds, FeatureKeys.NotificationsEmail, now, cancellationToken);
        var whatsAppAccounts = await EnabledAccountsAsync(context, accountIds, FeatureKeys.NotificationsWhatsApp, now, cancellationToken);

        var processed = 0;
        var failed = 0;
        foreach (var delivery in eligible)
        {
            if (!notificationsAccounts.Contains(delivery.AccountId)
                || (delivery.Channel == NotificationChannels.Email && !emailAccounts.Contains(delivery.AccountId))
                || (delivery.Channel == NotificationChannels.WhatsApp && !whatsAppAccounts.Contains(delivery.AccountId)))
            {
                continue; // held: feature disabled, configuration preserved
            }

            try
            {
                context.NotificationDeliveries.Attach(delivery);
                delivery.Status = DeliveryStatuses.Sending;
                await context.SaveChangesAsync(cancellationToken);

                var result = await DispatchAsync(context, providers, delivery, cancellationToken);
                if (result.Success)
                {
                    delivery.Status = DeliveryStatuses.Sent;
                    delivery.SentAt = DateTimeOffset.UtcNow;
                    delivery.ProviderMessageId = result.ProviderMessageId;
                    delivery.Error = null;
                    delivery.Attempts += 1;
                }
                else
                {
                    failed += ApplyFailure(context, delivery, result.Error ?? "Unknown provider error.");
                }

                await context.SaveChangesAsync(cancellationToken);
                await PublishOutcomeAsync(publisher, delivery, cancellationToken);
                processed++;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dispatch failed for delivery {DeliveryId} ({Channel}).", delivery.NotificationDeliveryId, delivery.Channel);
                failed += ApplyFailure(context, delivery, ex.Message);
                await context.SaveChangesAsync(cancellationToken);
                await PublishOutcomeAsync(publisher, delivery, cancellationToken);
                processed++;
            }
        }

        if (processed > 0)
        {
            context.BackgroundJobRuns.Add(new BackgroundJobRun(
                JobKey, null, processed.ToString(), $"dispatch:{now:yyyyMMddHHmmssfff}", "Succeeded", 1, now)
            {
                CompletedAt = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Notification dispatch cycle: {Processed} delivery(ies) processed, {Failed} permanently failed.", processed, failed);
        }
    }

    // Crash recovery. A delivery is flipped to Sending and committed BEFORE the provider call, and no
    // other code path moves it back: the eligibility query above only sees Pending, retention only
    // deletes Sent/Failed/Digested, and a manual retry refuses anything that is not Failed. An
    // in-process exception unwinds through ApplyFailure, but a hard stop — a crash, or the
    // cancellation rethrow during graceful shutdown — does not, so without this the in-flight
    // delivery of every restart is stranded forever. Anything sitting in Sending past the reclaim
    // window is put back through the ordinary failure path, which counts the attempt so the backoff
    // ladder applies and MaxAttempts still terminates a delivery that keeps killing the process.
    private async Task ReclaimStrandedAsync(ApplicationDbContext context, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var reclaimMinutes = Math.Max(1, configuration.GetValue<int?>("AppSettings:NotificationSendingReclaimMinutes") ?? DefaultSendingReclaimMinutes);
        var cutoff = now.AddMinutes(-reclaimMinutes);

        var stranded = await context.NotificationDeliveries
            .Where(d => d.Status == DeliveryStatuses.Sending && d.LastModified <= cutoff)
            .OrderBy(d => d.LastModified)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (stranded.Count == 0)
        {
            return;
        }

        foreach (var delivery in stranded)
        {
            context.NotificationDeliveries.Attach(delivery);
            ApplyFailure(context, delivery, $"Dispatch was interrupted: the delivery stayed in {DeliveryStatuses.Sending} for more than {reclaimMinutes} minute(s).");
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogWarning("Reclaimed {Count} delivery(ies) stranded in {Status} for more than {Minutes} minute(s).",
            stranded.Count, DeliveryStatuses.Sending, reclaimMinutes);
    }

    // Domain events: a publish failure must never disturb the dispatch loop.
    private async Task PublishOutcomeAsync(IPublisher publisher, NotificationDelivery delivery, CancellationToken cancellationToken)
    {
        try
        {
            if (delivery.Status == DeliveryStatuses.Sent)
            {
                await publisher.Publish(new NotificationDeliverySucceeded.Notification(
                    delivery.NotificationDeliveryId, delivery.AccountId, delivery.Channel, delivery.ProviderMessageId), cancellationToken);
            }
            else if (delivery.Status == DeliveryStatuses.Failed)
            {
                await publisher.Publish(new NotificationDeliveryFailed.Notification(
                    delivery.NotificationDeliveryId, delivery.AccountId, delivery.Channel, delivery.Attempts, delivery.Error), cancellationToken);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to publish delivery outcome for {DeliveryId}.", delivery.NotificationDeliveryId);
        }
    }

    // Returns 1 when the delivery permanently failed (for cycle accounting), 0 for a retryable failure.
    private static int ApplyFailure(ApplicationDbContext context, NotificationDelivery delivery, string error)
    {
        delivery.Attempts += 1;
        delivery.Error = error;
        if (delivery.Attempts >= MaxAttempts)
        {
            delivery.Status = DeliveryStatuses.Failed;
            // Delivery-failure alert. Recorded directly (no rule evaluation) so a
            // failing channel can never notify itself into a loop; it stays visible in the alert feed.
            context.AlertEvents.Add(new AlertEvent(
                delivery.AccountId, AlertEventTypes.NotificationDeliveryFailed, AlertSeverities.Warning,
                "Notifications", "NotificationDelivery", delivery.NotificationDeliveryId.ToString(), "Open",
                JsonSerializer.Serialize(new { delivery.Channel, delivery.Attempts, Error = error }),
                $"delivery-failed:{delivery.NotificationDeliveryId:N}"));
            return 1;
        }

        delivery.Status = DeliveryStatuses.Pending;
        return 0;
    }

    private async Task<NotificationSendResult> DispatchAsync(
        ApplicationDbContext context,
        IReadOnlyDictionary<string, INotificationChannelProvider> providers,
        NotificationDelivery delivery,
        CancellationToken cancellationToken)
    {
        if (!providers.TryGetValue(delivery.Channel, out var provider))
        {
            // No registered provider (e.g. Push): fail terminally, no pointless retries.
            delivery.Attempts = MaxAttempts - 1;
            return new NotificationSendResult(false, null, $"No delivery provider is registered for channel '{delivery.Channel}'.");
        }

        var message = await BuildMessageAsync(context, delivery, cancellationToken);
        return await provider.SendAsync(message, cancellationToken);
    }

    private async Task<NotificationMessage> BuildMessageAsync(ApplicationDbContext context, NotificationDelivery delivery, CancellationToken cancellationToken)
    {
        var portalBaseUrl = configuration.GetValue<string>("AppSettings:PortalBaseUrl") ?? "https://localhost:3000";

        NotificationRule? rule = delivery.NotificationRuleId.HasValue
            ? await context.NotificationRules.FirstOrDefaultAsync(r => r.NotificationRuleId == delivery.NotificationRuleId.Value, cancellationToken)
            : null;
        AlertEvent? alertEvent = delivery.AlertEventId.HasValue
            ? await context.AlertEvents.FirstOrDefaultAsync(e => e.AlertEventId == delivery.AlertEventId.Value, cancellationToken)
            : null;

        var ruleConfiguration = NotificationRuleContracts.ParseConfiguration(rule?.ConfigurationJson);

        // In-app deliveries carry no server-rendered text: the portal localizes them from the
        // event metadata through its own i18n layer.
        if (delivery.Channel == NotificationChannels.InApp)
        {
            return new NotificationMessage(delivery.NotificationDeliveryId, delivery.AccountId, delivery.Recipient,
                null, string.Empty, NotificationLocales.English, null, alertEvent?.PayloadJson);
        }

        // Localization comes from the recipient: their UserSettings language when the recipient is
        // a user, else the rule's configured locale, else English.
        var locale = await NotificationLocaleResolver.ResolveAsync(context, delivery.RecipientPrincipalType, delivery.Recipient, ruleConfiguration.Locale, cancellationToken);

        // Pre-rendered content (digest summaries) bypasses template resolution.
        if (!string.IsNullOrWhiteSpace(delivery.PayloadJson))
        {
            var prerendered = ParsePrerendered(delivery.PayloadJson);
            if (prerendered.HasValue)
            {
                return new NotificationMessage(delivery.NotificationDeliveryId, delivery.AccountId, delivery.Recipient,
                    prerendered.Value.Subject, prerendered.Value.Body, locale, ruleConfiguration.WebhookSecret, alertEvent?.PayloadJson);
            }
        }

        var templateKey = alertEvent?.EventType ?? NotificationMessageRenderer.TestTemplateKey;
        var tokens = new Dictionary<string, string>
        {
            ["eventType"] = alertEvent?.EventType ?? "Test",
            ["severity"] = alertEvent?.Severity ?? "Info",
            ["sourceModule"] = alertEvent?.SourceModule ?? "Notifications",
            ["resourceType"] = alertEvent?.ResourceType ?? string.Empty,
            ["resourceId"] = alertEvent?.ResourceId ?? string.Empty,
            ["occurredAt"] = (alertEvent?.LastSeenAt ?? DateTimeOffset.UtcNow).ToString("O"),
            ["link"] = portalBaseUrl
        };

        var (subject, body) = await NotificationMessageRenderer.RenderAsync(context, delivery.AccountId, templateKey, delivery.Channel, locale, tokens, cancellationToken);
        return new NotificationMessage(delivery.NotificationDeliveryId, delivery.AccountId, delivery.Recipient,
            subject, body, locale, ruleConfiguration.WebhookSecret, alertEvent?.PayloadJson);
    }

    private static (string? Subject, string Body)? ParsePrerendered(string payloadJson)
    {
        try
        {
            using var document = JsonDocument.Parse(payloadJson);
            if (document.RootElement.TryGetProperty("body", out var body) && body.ValueKind == JsonValueKind.String)
            {
                var subject = document.RootElement.TryGetProperty("subject", out var s) && s.ValueKind == JsonValueKind.String ? s.GetString() : null;
                return (subject, body.GetString()!);
            }
        }
        catch (JsonException)
        {
            // fall through to template rendering
        }
        return null;
    }

    private static async Task<HashSet<Guid>> EnabledAccountsAsync(ApplicationDbContext context, IReadOnlyCollection<Guid> accountIds, string featureKey, DateTimeOffset now, CancellationToken cancellationToken)
        => [.. await context.AccountFeatures
            .Where(f => accountIds.Contains(f.AccountId) && f.FeatureKey == featureKey && f.Enabled
                && (f.EffectiveFrom == null || f.EffectiveFrom <= now)
                && (f.EffectiveTo == null || f.EffectiveTo >= now))
            .Select(f => f.AccountId)
            .ToListAsync(cancellationToken)];
}
