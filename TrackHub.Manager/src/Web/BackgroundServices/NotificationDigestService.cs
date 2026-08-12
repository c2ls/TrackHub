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
using Microsoft.EntityFrameworkCore;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.ManagerDB.Notifications;

namespace TrackHub.Manager.Web.BackgroundServices;

// Digest job: hourly, folds Deferred deliveries into one pre-rendered summary
// delivery per (rule, recipient, channel) and marks the originals Digested. Rules with a Daily
// cadence fold at most once per 24 h (tracked by the previous summary for the same group).
public sealed class NotificationDigestService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<NotificationDigestService> logger) : BackgroundService
{
    private const string JobKey = BackgroundJobKeys.NotificationDigest;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await RunOnceAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "Notification digest cycle failed."); }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTimeOffset.UtcNow;
        var portalBaseUrl = configuration.GetValue<string>("AppSettings:PortalBaseUrl") ?? "https://localhost:3000";

        var deferred = await context.NotificationDeliveries
            .Where(d => d.Status == DeliveryStatuses.Deferred && d.NotificationRuleId != null)
            .ToListAsync(cancellationToken);
        if (deferred.Count == 0)
        {
            return;
        }

        var ruleIds = deferred.Select(d => d.NotificationRuleId!.Value).Distinct().ToList();
        var rules = await context.NotificationRules
            .Where(r => ruleIds.Contains(r.NotificationRuleId))
            .ToDictionaryAsync(r => r.NotificationRuleId, cancellationToken);

        // Disabling `notifications` stops dispatch for the account — folding a digest
        // is dispatch preparation, so those groups are held until the feature is re-enabled.
        var digestAccountIds = deferred.Select(d => d.AccountId).Distinct().ToList();
        var enabledAccounts = await context.AccountFeatures
            .Where(f => digestAccountIds.Contains(f.AccountId) && f.FeatureKey == FeatureKeys.Notifications && f.Enabled
                && (f.EffectiveFrom == null || f.EffectiveFrom <= now)
                && (f.EffectiveTo == null || f.EffectiveTo >= now))
            .Select(f => f.AccountId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var summaries = 0;
        foreach (var group in deferred.GroupBy(d => (RuleId: d.NotificationRuleId!.Value, d.RecipientPrincipalType, d.Recipient, d.Channel)))
        {
            try
            {
                if (!rules.TryGetValue(group.Key.RuleId, out var rule) || !enabledAccounts.Contains(rule.AccountId))
                {
                    continue;
                }

                var throttling = NotificationRuleContracts.ParseThrottling(rule.ThrottlingJson);
                if (throttling.Digest == DigestCadences.Daily && await SummaryExistsWithinAsync(context, group.Key, now.AddHours(-24), cancellationToken))
                {
                    continue; // daily cadence: at most one summary per 24 h per group
                }

                var eventIds = group.Where(d => d.AlertEventId.HasValue).Select(d => d.AlertEventId!.Value).Distinct().ToList();
                var eventTypes = eventIds.Count > 0
                    ? await context.AlertEvents.Where(e => eventIds.Contains(e.AlertEventId)).Select(e => e.EventType).Distinct().ToListAsync(cancellationToken)
                    : [];

                // Recipient-driven locale (same rule as the dispatcher): the recipient's own
                // UserSettings language when it is a user, else the rule locale, else English.
                var locale = await NotificationLocaleResolver.ResolveAsync(context,
                    group.Key.RecipientPrincipalType, group.Key.Recipient,
                    NotificationRuleContracts.ParseConfiguration(rule.ConfigurationJson).Locale, cancellationToken);
                var tokens = new Dictionary<string, string>
                {
                    ["count"] = group.Count().ToString(),
                    ["eventTypes"] = string.Join(", ", eventTypes),
                    ["link"] = portalBaseUrl
                };
                var (subject, body) = await NotificationMessageRenderer.RenderAsync(
                    context, rule.AccountId, NotificationMessageRenderer.DigestTemplateKey, group.Key.Channel, locale, tokens, cancellationToken);

                context.NotificationDeliveries.Add(new NotificationDelivery(
                    rule.AccountId, rule.NotificationRuleId, null, group.Key.Channel,
                    group.Key.RecipientPrincipalType, group.Key.Recipient, DeliveryStatuses.Pending)
                {
                    PayloadJson = JsonSerializer.Serialize(new { subject, body })
                });

                foreach (var delivery in group)
                {
                    context.NotificationDeliveries.Attach(delivery);
                    delivery.Status = DeliveryStatuses.Digested;
                }

                await context.SaveChangesAsync(cancellationToken);
                summaries++;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Digest fold failed for rule {RuleId} recipient {Recipient}.", group.Key.RuleId, group.Key.Recipient);
            }
        }

        if (summaries > 0)
        {
            context.BackgroundJobRuns.Add(new BackgroundJobRun(
                JobKey, null, summaries.ToString(), $"digest:{now:yyyyMMddHHmmssfff}", "Succeeded", 1, now)
            {
                CompletedAt = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Digest cycle folded deferred deliveries into {Count} summary delivery(ies).", summaries);
        }
    }

    private static async Task<bool> SummaryExistsWithinAsync(ApplicationDbContext context, (Guid RuleId, string RecipientPrincipalType, string Recipient, string Channel) key, DateTimeOffset since, CancellationToken cancellationToken)
        => await context.NotificationDeliveries.AnyAsync(d =>
            d.NotificationRuleId == key.RuleId
            && d.AlertEventId == null
            && d.PayloadJson != null
            && d.RecipientPrincipalType == key.RecipientPrincipalType
            && d.Recipient == key.Recipient
            && d.Channel == key.Channel
            && d.Created >= since, cancellationToken);
}
