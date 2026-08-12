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
using System.Text.Json;
using Common.Domain.Enums;
using Common.Mediator;
using Microsoft.EntityFrameworkCore;
using TrackHub.Manager.Application.Accounts.Events;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Web.BackgroundServices;

// Trial-expiration enforcement: once per cycle, transitions Trial accounts past
// their trial-end to Suspended, records a BackgroundJobRun, and raises AccountStatusChanged. Bounded,
// idempotent (unique {JobKey, IdempotencyKey}), and a no-op when no trial-end data exists. Trial-end
// is read from AccountFeature configuration (`trialEndsAt`) or a trial-tier feature's EffectiveTo.
// Runs host-internally against the DB directly (no per-account principal), like the Telemetry purge job.
public sealed class TrialExpirationService(
    IServiceScopeFactory scopeFactory,
    ILogger<TrialExpirationService> logger) : BackgroundService
{
    private const string JobKey = BackgroundJobKeys.TrialExpiration;
    private const string TrialTier = "trial";
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(StartupDelay, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Trial-expiration cycle failed.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var now = DateTimeOffset.UtcNow;
        var trialStatus = (short)AccountStatus.Trial;

        var trialAccounts = await context.Accounts
            .Where(a => a.Status == trialStatus)
            .Select(a => a.AccountId)
            .ToListAsync(cancellationToken);

        var suspended = 0;
        foreach (var accountId in trialAccounts)
        {
            var trialEnd = await ResolveTrialEndAsync(context, accountId, cancellationToken);
            if (trialEnd is null || trialEnd.Value > now)
            {
                continue; // no trial-end data or not yet expired → no-op
            }

            var idempotencyKey = $"trial-expire:{accountId:N}:{trialEnd.Value:yyyyMMdd}";
            var alreadyRun = await context.BackgroundJobRuns.AnyAsync(
                r => r.JobKey == JobKey && r.IdempotencyKey == idempotencyKey && r.Status == "Succeeded",
                cancellationToken);
            if (alreadyRun)
            {
                continue; // re-run creates no duplicate transition
            }

            try
            {
                if (await SuspendAsync(context, publisher, accountId, trialEnd.Value, idempotencyKey, now, cancellationToken))
                {
                    suspended++;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Trial-expiration failed for account {AccountId}.", accountId);
                await RecordFailureAsync(accountId, idempotencyKey, now, ex, cancellationToken);
            }
        }

        logger.LogInformation(
            "Trial-expiration cycle complete: {Total} trial account(s) checked, {Suspended} suspended.",
            trialAccounts.Count, suspended);
    }

    private async Task<bool> SuspendAsync(
        ApplicationDbContext context, IPublisher publisher, Guid accountId,
        DateTimeOffset trialEnd, string idempotencyKey, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var account = await context.Accounts.FirstAsync(a => a.AccountId == accountId, cancellationToken);
        context.Accounts.Attach(account);

        var previous = (AccountStatus)account.Status;
        if (previous != AccountStatus.Trial)
        {
            return false; // status changed since selection; leave it alone
        }

        const string reason = "Trial period expired.";
        account.Status = (short)AccountStatus.Suspended;
        account.Active = false;
        account.StatusChangedAt = now;

        context.AuditEvents.Add(new AuditEvent(
            accountId, "System", JobKey, "AccountStatusChanged", "Account", accountId.ToString(),
            "Succeeded",
            $"{{\"status\":\"{previous}\"}}",
            $"{{\"status\":\"{AccountStatus.Suspended}\",\"reason\":\"{reason}\"}}",
            reason, null, null, null));

        context.BackgroundJobRuns.Add(new BackgroundJobRun(
            JobKey, accountId, accountId.ToString(), idempotencyKey, "Succeeded", 1, now)
        {
            CompletedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new AccountStatusChanged.Notification(
            accountId, previous, AccountStatus.Suspended, reason, JobKey, null), cancellationToken);

        logger.LogInformation("Trial expired: account {AccountId} suspended (trial ended {TrialEnd:O}).", accountId, trialEnd);
        return true;
    }

    private async Task RecordFailureAsync(
        Guid accountId, string idempotencyKey, DateTimeOffset startedAt, Exception ex, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.BackgroundJobRuns.Add(new BackgroundJobRun(
                JobKey, accountId, accountId.ToString(), $"{idempotencyKey}:failed:{startedAt:O}", "Failed", 1, startedAt)
            {
                CompletedAt = DateTimeOffset.UtcNow,
                ErrorCode = ex.GetType().Name,
                ErrorMessage = ex.Message
            });
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception recordEx)
        {
            logger.LogError(recordEx, "Failed to record trial-expiration failure for account {AccountId}.", accountId);
        }
    }

    private static async Task<DateTimeOffset?> ResolveTrialEndAsync(
        ApplicationDbContext context, Guid accountId, CancellationToken cancellationToken)
    {
        var features = await context.AccountFeatures
            .Where(f => f.AccountId == accountId)
            .Select(f => new { f.Tier, f.EffectiveTo, f.ConfigurationJson })
            .ToListAsync(cancellationToken);

        DateTimeOffset? trialEnd = null;
        foreach (var feature in features)
        {
            var fromJson = ParseTrialEnd(feature.ConfigurationJson);
            if (fromJson.HasValue)
            {
                trialEnd = Earliest(trialEnd, fromJson);
            }
            else if (string.Equals(feature.Tier, TrialTier, StringComparison.OrdinalIgnoreCase) && feature.EffectiveTo.HasValue)
            {
                trialEnd = Earliest(trialEnd, feature.EffectiveTo);
            }
        }

        return trialEnd;
    }

    private static DateTimeOffset? ParseTrialEnd(string? configurationJson)
    {
        if (string.IsNullOrWhiteSpace(configurationJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(configurationJson);
            return doc.RootElement.TryGetProperty("trialEndsAt", out var value)
                && value.TryGetDateTimeOffset(out var trialEnd)
                ? trialEnd
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static DateTimeOffset? Earliest(DateTimeOffset? current, DateTimeOffset? candidate)
    {
        if (!current.HasValue)
        {
            return candidate;
        }

        if (!candidate.HasValue)
        {
            return current;
        }

        return candidate.Value < current.Value ? candidate : current;
    }
}
