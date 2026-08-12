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
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Web.BackgroundServices;

// Alert evaluation job: every 5 minutes detects communication loss (stale
// transporter positions) for accounts with an enabled CommunicationLoss rule, escalates
// unacknowledged critical alerts once, and once a day emits GPS credential-expiry alerts
// in-process (retiring the need for an external ServiceClient caller; the mutation remains for
// manual runs). Skips feature-disabled and suspended accounts. Runs host-internally against the
// DB directly (no per-account principal), like the sibling jobs.
public sealed class AlertEvaluationService(
    IServiceScopeFactory scopeFactory,
    ILogger<AlertEvaluationService> logger) : BackgroundService
{
    private const string JobKey = BackgroundJobKeys.AlertEvaluation;
    private const int DefaultCommunicationLossThresholdMinutes = 60;
    private const int CredentialExpiryWithinDays = 7;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await RunOnceAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "Alert evaluation cycle failed."); }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var evaluator = scope.ServiceProvider.GetRequiredService<IAlertRuleEvaluator>();

        var now = DateTimeOffset.UtcNow;
        var enabledAccounts = await FeatureEnabledActiveAccountsAsync(context, FeatureKeys.Notifications, now, cancellationToken);

        if (enabledAccounts.Count > 0)
        {
            await DetectCommunicationLossAsync(context, evaluator, enabledAccounts, now, cancellationToken);
            await EscalateUnacknowledgedCriticalAsync(context, enabledAccounts, now, cancellationToken);
        }

        await EmitCredentialExpiryDailyAsync(context, evaluator, now, cancellationToken);
    }

    private async Task DetectCommunicationLossAsync(ApplicationDbContext context, IAlertRuleEvaluator evaluator, IReadOnlyCollection<Guid> accountIds, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var rules = await context.NotificationRules
            .Where(r => accountIds.Contains(r.AccountId) && r.Enabled && r.TriggerEvent == AlertEventTypes.CommunicationLoss)
            .ToListAsync(cancellationToken);

        var raised = 0;
        foreach (var rule in rules)
        {
            try
            {
                var thresholdMinutes = NotificationRuleContracts.ParseConfiguration(rule.ConfigurationJson).ThresholdMinutes ?? DefaultCommunicationLossThresholdMinutes;
                var cutoff = now.AddMinutes(-Math.Max(1, thresholdMinutes));

                // A transporter that never reported is a provisioning state, not communication loss.
                var stale = await context.TransporterPositions
                    .Where(p => p.Transporter.AccountId == rule.AccountId && p.DeviceDateTime <= cutoff)
                    .Select(p => new { p.TransporterId, p.Transporter.Name, p.DeviceDateTime })
                    .ToListAsync(cancellationToken);

                foreach (var transporter in stale)
                {
                    var alertEvent = await RecordDedupedAsync(context, rule.AccountId,
                        AlertEventTypes.CommunicationLoss, AlertSeverities.Warning, "Notifications",
                        "Transporter", transporter.TransporterId.ToString(),
                        JsonSerializer.Serialize(new { transporter.TransporterId, transporter.Name, LastPositionAt = transporter.DeviceDateTime, ThresholdMinutes = thresholdMinutes }),
                        $"comm-loss:{transporter.TransporterId:N}:{now:yyyyMMdd}",
                        cancellationToken);
                    if (alertEvent is not null)
                    {
                        await evaluator.EvaluateAsync(alertEvent.Value, cancellationToken);
                        raised++;
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Communication-loss detection failed for rule {RuleKey} (account {AccountId}).", rule.RuleKey, rule.AccountId);
            }
        }

        if (raised > 0)
        {
            context.BackgroundJobRuns.Add(new BackgroundJobRun(
                JobKey, null, raised.ToString(), $"comm-loss:{now:yyyyMMddHHmmssfff}", "Succeeded", 1, now)
            {
                CompletedAt = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Communication-loss detection raised {Count} alert(s).", raised);
        }
    }

    // Single-step deterministic escalation: a critical alert unacknowledged past
    // the rule's escalateAfterMinutes gets exactly one role-addressed InApp delivery to administrators.
    private async Task EscalateUnacknowledgedCriticalAsync(ApplicationDbContext context, IReadOnlyCollection<Guid> accountIds, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var criticalOpen = await context.AlertEvents
            .Where(e => accountIds.Contains(e.AccountId) && e.Status == "Open" && e.Severity == AlertSeverities.Critical)
            .ToListAsync(cancellationToken);
        if (criticalOpen.Count == 0)
        {
            return;
        }

        var eventTypes = criticalOpen.Select(e => e.EventType).Distinct().ToList();
        var rules = await context.NotificationRules
            .Where(r => accountIds.Contains(r.AccountId) && r.Enabled && eventTypes.Contains(r.TriggerEvent))
            .ToListAsync(cancellationToken);

        var escalated = 0;
        foreach (var alertEvent in criticalOpen)
        {
            try
            {
                var rule = rules.FirstOrDefault(r => r.AccountId == alertEvent.AccountId && r.TriggerEvent == alertEvent.EventType);
                if (rule is null)
                {
                    continue;
                }

                var escalateAfterMinutes = NotificationRuleContracts.ParseConfiguration(rule.ConfigurationJson).EscalateAfterMinutes;
                if (escalateAfterMinutes is null || alertEvent.FirstSeenAt > now.AddMinutes(-escalateAfterMinutes.Value))
                {
                    continue;
                }

                var idempotencyKey = $"escalate:{alertEvent.AlertEventId:N}";
                var alreadyEscalated = await context.BackgroundJobRuns.AnyAsync(
                    r => r.JobKey == JobKey && r.IdempotencyKey == idempotencyKey && r.Status == "Succeeded", cancellationToken);
                if (alreadyEscalated)
                {
                    continue;
                }

                context.NotificationDeliveries.Add(new NotificationDelivery(
                    alertEvent.AccountId, rule.NotificationRuleId, alertEvent.AlertEventId,
                    NotificationChannels.InApp, RecipientPrincipalTypes.Role, Roles.Administrator, DeliveryStatuses.Pending));
                context.BackgroundJobRuns.Add(new BackgroundJobRun(
                    JobKey, alertEvent.AccountId, alertEvent.AlertEventId.ToString(), idempotencyKey, "Succeeded", 1, now)
                {
                    CompletedAt = DateTimeOffset.UtcNow
                });
                await context.SaveChangesAsync(cancellationToken);
                escalated++;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Escalation failed for alert event {AlertEventId}.", alertEvent.AlertEventId);
            }
        }

        if (escalated > 0)
        {
            logger.LogInformation("Escalated {Count} unacknowledged critical alert(s) to administrators.", escalated);
        }
    }

    // Daily in-process credential-expiry emission: mirrors
    // EmitExpiringCredentialAlertsCommand (same dedup keys, so manual runs coalesce) for accounts
    // with gps.integration enabled.
    private async Task EmitCredentialExpiryDailyAsync(ApplicationDbContext context, IAlertRuleEvaluator evaluator, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var idempotencyKey = $"credential-scan:{now:yyyyMMdd}";
        var alreadyRan = await context.BackgroundJobRuns.AnyAsync(
            r => r.JobKey == JobKey && r.IdempotencyKey == idempotencyKey && r.Status == "Succeeded", cancellationToken);
        if (alreadyRan)
        {
            return;
        }

        var gpsAccounts = await FeatureEnabledActiveAccountsAsync(context, FeatureKeys.GpsIntegration, now, cancellationToken);
        var cutoff = now.AddDays(CredentialExpiryWithinDays);
        var emitted = 0;

        if (gpsAccounts.Count > 0)
        {
            var expiring = await context.Credentials
                .Where(c => gpsAccounts.Contains(c.Operator.AccountId)
                    && ((c.TokenExpiration.HasValue && c.TokenExpiration <= cutoff)
                        || (c.RefreshTokenExpiration.HasValue && c.RefreshTokenExpiration <= cutoff)))
                .Select(c => new { c.CredentialId, c.OperatorId, c.Operator.AccountId, c.TokenExpiration, c.RefreshTokenExpiration })
                .ToListAsync(cancellationToken);

            foreach (var credential in expiring)
            {
                try
                {
                    var alertEvent = await RecordDedupedAsync(context, credential.AccountId,
                        AlertEventTypes.GpsCredentialExpiring, AlertSeverities.Warning, "GpsIntegration",
                        "Operator", credential.OperatorId.ToString(),
                        JsonSerializer.Serialize(new { credential.CredentialId, credential.OperatorId, credential.TokenExpiration, credential.RefreshTokenExpiration, WithinDays = CredentialExpiryWithinDays }),
                        $"gps-credential-expiring:{credential.OperatorId:N}:{now:yyyyMMdd}",
                        cancellationToken);
                    if (alertEvent is not null)
                    {
                        await evaluator.EvaluateAsync(alertEvent.Value, cancellationToken);
                        emitted++;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Credential-expiry alert failed for operator {OperatorId}.", credential.OperatorId);
                }
            }
        }

        context.BackgroundJobRuns.Add(new BackgroundJobRun(
            JobKey, null, emitted.ToString(), idempotencyKey, "Succeeded", 1, now)
        {
            CompletedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Daily credential-expiry scan emitted {Count} alert(s).", emitted);
    }

    // Mirrors the AlertEventWriter dedup rule: (AccountId, DeduplicationKey, Status != Resolved)
    // coalesces into the existing open event. Returns the Vm to evaluate (null when only touched).
    private static async Task<AlertEventVm?> RecordDedupedAsync(
        ApplicationDbContext context, Guid accountId, string eventType, string severity, string sourceModule,
        string resourceType, string resourceId, string payloadJson, string deduplicationKey, CancellationToken cancellationToken)
    {
        var existing = await context.AlertEvents.FirstOrDefaultAsync(
            e => e.AccountId == accountId && e.DeduplicationKey == deduplicationKey && e.Status != "Resolved", cancellationToken);
        if (existing is not null)
        {
            context.AlertEvents.Attach(existing);
            existing.LastSeenAt = DateTimeOffset.UtcNow;
            existing.PayloadJson = payloadJson;
            await context.SaveChangesAsync(cancellationToken);
            return null;
        }

        var entity = new AlertEvent(accountId, eventType, severity, sourceModule, resourceType, resourceId, "Open", payloadJson, deduplicationKey);
        context.AlertEvents.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return new AlertEventVm(entity.AlertEventId, entity.AccountId, entity.EventType, entity.Severity, entity.SourceModule,
            entity.ResourceType, entity.ResourceId, entity.Status, entity.FirstSeenAt, entity.LastSeenAt, entity.PayloadJson,
            entity.DeduplicationKey, entity.LastModified);
    }

    private static async Task<List<Guid>> FeatureEnabledActiveAccountsAsync(ApplicationDbContext context, string featureKey, DateTimeOffset now, CancellationToken cancellationToken)
        => await context.AccountFeatures
            .Where(f => f.FeatureKey == featureKey && f.Enabled
                && (f.EffectiveFrom == null || f.EffectiveFrom <= now)
                && (f.EffectiveTo == null || f.EffectiveTo >= now)
                // Suspended accounts are skipped.
                && context.Accounts.Any(a => a.AccountId == f.AccountId && a.Active))
            .Select(f => f.AccountId)
            .Distinct()
            .ToListAsync(cancellationToken);
}
