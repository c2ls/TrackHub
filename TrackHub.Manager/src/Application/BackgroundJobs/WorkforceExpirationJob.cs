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
using Common.Domain.Time;
using Microsoft.Extensions.Logging;
using TrackHub.Manager.Application.AlertEvents;
using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Application.BackgroundJobs;

/// <summary>
/// Raises exactly one qualification alert per (qualification, expiry, threshold) across the 30/15/7/0-day
/// bands (spec 09 §10). Idempotency lives in BackgroundJobRun, so a threshold is never re-notified,
/// including across restarts. Qualification alerting is a `workforce` capability (AC6).
/// </summary>
public sealed class WorkforceExpirationJob(
    IAccountFeatureGate features,
    IWorkforceExpirationStore store,
    IAccountTimeZoneResolver zones,
    IAlertRecorder recorder,
    ILogger<WorkforceExpirationJob> logger) : IScheduledJob
{
    public static TimeSpan Interval => TimeSpan.FromHours(24);
    public static TimeSpan StartupDelay => TimeSpan.FromMinutes(3);

    public async Task RunOnceAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var enabledAccounts = await features.EnabledAccountsAsync(FeatureKeys.Workforce, now, cancellationToken);
        if (enabledAccounts.Count == 0)
        {
            return;
        }

        // The candidate scan uses a UTC day with one day of slack; the band each qualification
        // crossed is decided against ITS account's calendar below.
        var horizon = AccountTimeZone.Utc.DateOf(now).AddDays(WorkforceLimits.ExpirationThresholdsDays.Max() + 1);
        var candidates = await store.GetExpiringAsync(enabledAccounts, horizon, cancellationToken);

        var raised = 0;
        foreach (var qualification in candidates)
        {
            var accountToday = (await zones.ResolveAsync(qualification.AccountId, cancellationToken)).DateOf(now);
            var threshold = ThresholdFor(qualification.ExpiresAt, accountToday);
            if (threshold is null)
            {
                continue;
            }

            // One bad qualification must not abandon the rest of the batch until tomorrow's tick.
            try
            {
                if (await TryRaiseAsync(qualification, threshold.Value, now, cancellationToken))
                {
                    raised++;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to raise the expiration alert for qualification {QualificationId}; continuing the batch.",
                    qualification.StaffQualificationId);
            }
        }

        if (raised > 0)
        {
            logger.LogInformation("Workforce expiration cycle: {Raised} qualification alert(s) across {Accounts} account(s).", raised, enabledAccounts.Count);
        }
    }

    /// <summary>
    /// The nearest crossed band, so a qualification that entered the 7-day window never later
    /// back-fires the 15/30-day alerts. Null when no band applies yet.
    /// </summary>
    public static int? ThresholdFor(DateOnly expiresAt, DateOnly today)
    {
        var daysLeft = expiresAt.DayNumber - today.DayNumber;
        var crossed = WorkforceLimits.ExpirationThresholdsDays.Where(t => daysLeft <= t).ToList();
        return crossed.Count == 0 ? null : crossed.Min();
    }

    private async Task<bool> TryRaiseAsync(
        ExpiringQualificationVm qualification, int threshold, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var idempotencyKey = $"{qualification.StaffQualificationId:N}:{qualification.ExpiresAt:yyyyMMdd}:{threshold}";
        if (await store.JobRunSucceededAsync(idempotencyKey, cancellationToken))
        {
            return false;
        }

        // Threshold 0 is the "already due" band — that is the Expired event, everything above is a warning.
        var isExpired = threshold == 0;

        // Serialized, not interpolated: QualificationType is only constrained by the command validators,
        // so a future import path or direct write could otherwise inject into the payload.
        var payloadJson = JsonSerializer.Serialize(new
        {
            threshold,
            qualificationType = qualification.QualificationType,
            staffId = qualification.StaffId,
            expiresAt = qualification.ExpiresAt.ToString("O"),
        });

        await recorder.RecordAsync(new AlertEventDto(
            qualification.AccountId,
            isExpired ? AlertEventTypes.StaffQualificationExpired : AlertEventTypes.StaffQualificationExpiring,
            isExpired ? AlertSeverities.High : AlertSeverities.Warning,
            "Workforce",
            "StaffQualification",
            qualification.StaffQualificationId.ToString(),
            "Open",
            payloadJson,
            AlertKeys.StaffQualification(qualification.StaffQualificationId, qualification.ExpiresAt, threshold)), cancellationToken);

        // The marker follows the emission: a crash in between repeats an alert the recorder folds, never loses one.
        await store.RecordJobRunAsync(qualification.AccountId, qualification.StaffQualificationId.ToString(), idempotencyKey, now, cancellationToken);
        return true;
    }
}
