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
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Web.BackgroundServices;

// Qualification-expiration scan (spec 09 §10). Daily, in-process against the DB: for accounts with the
// `workforce` feature enabled, raise exactly one alert event per (qualification, threshold) across the
// 30/15/7/0-day bands. Idempotency lives in BackgroundJobRun, so a threshold is never re-notified —
// including across restarts. Records a BackgroundJobRun row only when it actually raised something,
// which is why `workforce-expiration-scan` is NOT in BackgroundJobKeys.RecordsEveryCycle.
public sealed class WorkforceExpirationService(
    IServiceScopeFactory scopeFactory,
    ILogger<WorkforceExpirationService> logger) : BackgroundService
{
    private const string JobKey = BackgroundJobKeys.WorkforceExpirationScan;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(3);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await RunOnceAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "Workforce expiration cycle failed."); }

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
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var horizon = today.AddDays(WorkforceLimits.ExpirationThresholdsDays.Max());

        // Billing gate: qualification alerting is a `workforce` capability, so accounts without it are
        // skipped entirely (AC6).
        var enabledAccounts = await context.AccountFeatures
            .Where(f => f.FeatureKey == FeatureKeys.Workforce && f.Enabled
                && (f.EffectiveFrom == null || f.EffectiveFrom <= now)
                && (f.EffectiveTo == null || f.EffectiveTo >= now))
            .Select(f => f.AccountId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (enabledAccounts.Count == 0)
        {
            return;
        }

        // Revoked qualifications are already dead — expiry alerting would be noise.
        var candidates = await context.DriverQualifications
            .Where(q => enabledAccounts.Contains(q.AccountId)
                && q.Status != DriverQualificationStatuses.Revoked
                && q.ExpiresAt != null
                && q.ExpiresAt <= horizon)
            .Select(q => new { q.DriverQualificationId, q.AccountId, q.DriverId, q.QualificationType, ExpiresAt = q.ExpiresAt!.Value })
            .ToListAsync(cancellationToken);

        var raised = 0;

        foreach (var qualification in candidates)
        {
            // Raise only the nearest crossed band. Idempotency makes it exactly-once per band, and
            // picking the minimum stops a qualification that entered the 7-day window from later
            // back-firing the 15/30-day alerts.
            var daysLeft = qualification.ExpiresAt.DayNumber - today.DayNumber;
            var crossed = WorkforceLimits.ExpirationThresholdsDays.Where(t => daysLeft <= t).ToList();
            if (crossed.Count == 0)
            {
                continue;
            }

            // One bad qualification must not abandon the rest of the batch until tomorrow's tick.
            try
            {
                if (await TryRaiseAsync(context, evaluator, qualification.DriverQualificationId, qualification.AccountId,
                        qualification.DriverId, qualification.QualificationType, crossed.Min(), qualification.ExpiresAt, now, cancellationToken))
                {
                    raised++;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to raise the expiration alert for qualification {QualificationId}; continuing the batch.",
                    qualification.DriverQualificationId);
            }
        }

        if (raised > 0)
        {
            logger.LogInformation("Workforce expiration cycle: {Raised} qualification alert(s) across {Accounts} account(s).", raised, enabledAccounts.Count);
        }
    }

    private static async Task<bool> TryRaiseAsync(
        ApplicationDbContext context,
        IAlertRuleEvaluator evaluator,
        Guid qualificationId,
        Guid accountId,
        Guid driverId,
        string qualificationType,
        int threshold,
        DateOnly expiresAt,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var idempotencyKey = $"{qualificationId:N}:{threshold}";
        var already = await context.BackgroundJobRuns.AnyAsync(
            r => r.JobKey == JobKey && r.IdempotencyKey == idempotencyKey && r.Status == "Succeeded", cancellationToken);
        if (already)
        {
            return false;
        }

        // Threshold 0 is the "already due" band — that is the Expired event, everything above is a warning.
        var isExpired = threshold == 0;
        var eventType = isExpired ? AlertEventTypes.DriverQualificationExpired : AlertEventTypes.DriverQualificationExpiring;
        var dedupKey = $"driver-qual:{qualificationId:N}:{threshold}";

        // Serialized, not interpolated: qualificationType is only constrained by the command validators,
        // so a future import path or direct write could otherwise inject into the payload.
        var payloadJson = JsonSerializer.Serialize(new
        {
            threshold,
            qualificationType,
            driverId,
            expiresAt = expiresAt.ToString("O"),
        });

        var alertEvent = new AlertEvent(accountId, eventType, isExpired ? AlertSeverities.High : AlertSeverities.Warning,
            "Workforce", "DriverQualification", qualificationId.ToString(), "Open", payloadJson, dedupKey);

        // The alert event and its idempotency marker commit together, BEFORE notification fan-out, so
        // AC6 ("exactly one alert event per threshold") holds even if delivery fails. The trade-off is
        // deliberate: a failed fan-out leaves a recorded-but-undelivered alert, which is logged by the
        // caller — preferable to evaluating first and risking duplicate deliveries after a crash.
        context.AlertEvents.Add(alertEvent);
        context.BackgroundJobRuns.Add(new BackgroundJobRun(JobKey, accountId, qualificationId.ToString(), idempotencyKey, "Succeeded", 1, now) { CompletedAt = DateTimeOffset.UtcNow });
        await context.SaveChangesAsync(cancellationToken);

        await evaluator.EvaluateAsync(new AlertEventVm(alertEvent.AlertEventId, alertEvent.AccountId, alertEvent.EventType,
            alertEvent.Severity, alertEvent.SourceModule, alertEvent.ResourceType, alertEvent.ResourceId, alertEvent.Status,
            alertEvent.FirstSeenAt, alertEvent.LastSeenAt, alertEvent.PayloadJson, alertEvent.DeduplicationKey, alertEvent.LastModified), cancellationToken);
        return true;
    }
}
