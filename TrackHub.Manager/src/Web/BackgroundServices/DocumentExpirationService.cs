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

using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Web.BackgroundServices;

// Expiration scan: for Active documents crossing the 30/15/7-day thresholds, raise
// exactly one DocumentExpiring per (document, threshold) — idempotency key {documentId}:{threshold} so a
// threshold is never notified twice — and raise DocumentExpired past due. Runs host-internally against
// the DB directly. Skips accounts without the `documents` feature (billing gate).
public sealed class DocumentExpirationService(
    IServiceScopeFactory scopeFactory,
    ILogger<DocumentExpirationService> logger) : BackgroundService
{
    private const string JobKey = BackgroundJobKeys.DocumentExpiration;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(12);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await RunOnceAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "Document expiration cycle failed."); }

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
        var maxThreshold = now.AddDays(DocumentLimits.ExpirationThresholdsDays.Max());

        // Enabled-feature accounts only (security jobs like scan are exempt; this is a billing surface).
        var enabledAccounts = await context.AccountFeatures
            .Where(f => f.FeatureKey == FeatureKeys.Documents && f.Enabled
                && (f.EffectiveFrom == null || f.EffectiveFrom <= now)
                && (f.EffectiveTo == null || f.EffectiveTo >= now))
            .Select(f => f.AccountId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (enabledAccounts.Count == 0)
        {
            return;
        }

        var candidates = await context.Documents
            .Where(d => enabledAccounts.Contains(d.AccountId)
                && d.Status == DocumentStatuses.Active
                && d.ExpiresAt != null
                && d.ExpiresAt <= maxThreshold)
            .Select(d => new { d.DocumentId, d.AccountId, d.Category, ExpiresAt = d.ExpiresAt!.Value })
            .ToListAsync(cancellationToken);

        var expiringRaised = 0;
        var expiredRaised = 0;

        foreach (var doc in candidates)
        {
            // One bad document must not abandon the rest of the batch until the next tick.
            try
            {
                if (doc.ExpiresAt <= now)
                {
                    if (await TryRaiseAsync(context, evaluator, doc.DocumentId, doc.AccountId, doc.Category, "expired", doc.ExpiresAt, now, cancellationToken))
                    {
                        expiredRaised++;
                    }
                    continue;
                }

                // Raise only the nearest crossed threshold (smallest T with daysLeft <= T). Idempotency keeps
                // it exactly-once; picking the nearest band means a document that enters the 7-day window
                // never later back-fires the 15/30-day alerts.
                var daysLeft = (doc.ExpiresAt - now).TotalDays;
                var crossed = DocumentLimits.ExpirationThresholdsDays.Where(t => daysLeft <= t).ToList();
                if (crossed.Count > 0
                    && await TryRaiseAsync(context, evaluator, doc.DocumentId, doc.AccountId, doc.Category, crossed.Min().ToString(), doc.ExpiresAt, now, cancellationToken))
                {
                    expiringRaised++;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to raise the expiration alert for document {DocumentId}; continuing the batch.", doc.DocumentId);
            }
        }

        logger.LogInformation("Document expiration cycle: {Expiring} expiring alert(s), {Expired} expired alert(s) across {Accounts} account(s).",
            expiringRaised, expiredRaised, enabledAccounts.Count);
    }

    // Idempotency key {documentId}:{threshold} guarantees exactly-once per (document, threshold).
    //
    // The marker is the LAST thing written, after the fan-out and the Active → Expired transition have
    // both succeeded. Writing it first burned the key before the work was known to have happened, so a
    // throw in between left the document Active with its "expired" alert already claimed — and, the key
    // carrying no date, nothing ever retried it. The alert event is deduplicated on
    // (AccountId, DeduplicationKey, Status != Resolved) the way AlertEventWriter does it, and the
    // evaluator suppresses a repeat delivery for an alert event it has already fanned out, so a retry
    // after a partial failure resumes rather than duplicating.
    private static async Task<bool> TryRaiseAsync(ApplicationDbContext context, IAlertRuleEvaluator evaluator, Guid documentId, Guid accountId, string category, string threshold, DateTimeOffset expiresAt, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var idempotencyKey = $"{documentId:N}:{threshold}";
        var already = await context.BackgroundJobRuns.AnyAsync(
            r => r.JobKey == JobKey && r.IdempotencyKey == idempotencyKey && r.Status == "Succeeded", cancellationToken);
        if (already)
        {
            return false;
        }

        var isExpired = threshold == "expired";
        var eventType = isExpired ? "DocumentExpired" : "DocumentExpiring";
        var dedupKey = $"{eventType}:{documentId:N}:{threshold}";
        var payloadJson = $$"""{"threshold":"{{threshold}}","category":"{{category}}","expiresAt":"{{expiresAt:O}}"}""";

        var alertEvent = await context.AlertEvents.FirstOrDefaultAsync(
            e => e.AccountId == accountId && e.DeduplicationKey == dedupKey && e.Status != "Resolved", cancellationToken);
        if (alertEvent is null)
        {
            alertEvent = new AlertEvent(accountId, eventType, isExpired ? "High" : "Warning", "Documents", "Document", documentId.ToString(), "Open", payloadJson, dedupKey);
            context.AlertEvents.Add(alertEvent);
        }
        else
        {
            context.AlertEvents.Attach(alertEvent);
            alertEvent.LastSeenAt = DateTimeOffset.UtcNow;
            alertEvent.PayloadJson = payloadJson;
        }

        // Persisted before the fan-out: the deliveries the evaluator writes carry the alert event id.
        await context.SaveChangesAsync(cancellationToken);

        await evaluator.EvaluateAsync(new AlertEventVm(alertEvent.AlertEventId, alertEvent.AccountId, alertEvent.EventType,
            alertEvent.Severity, alertEvent.SourceModule, alertEvent.ResourceType, alertEvent.ResourceId, alertEvent.Status,
            alertEvent.FirstSeenAt, alertEvent.LastSeenAt, alertEvent.PayloadJson, alertEvent.DeduplicationKey, alertEvent.LastModified), cancellationToken);

        if (isExpired)
        {
            MarkExpired(context, await context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken));
        }

        context.BackgroundJobRuns.Add(new BackgroundJobRun(JobKey, accountId, documentId.ToString(), idempotencyKey, "Succeeded", 1, now) { CompletedAt = DateTimeOffset.UtcNow });
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void MarkExpired(ApplicationDbContext context, Document? document)
    {
        if (document is not null && document.Status == DocumentStatuses.Active)
        {
            context.Documents.Attach(document);
            document.Status = DocumentStatuses.Expired;
        }
    }
}
