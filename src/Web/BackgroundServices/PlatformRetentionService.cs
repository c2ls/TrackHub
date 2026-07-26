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
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Web.BackgroundServices;

// Platform-table retention: daily, purges the two append-only tables nothing else ages out —
// background_job_runs (notification-dispatch alone writes up to 2,880 rows a day) and resolved
// alert_events (the communication-loss and credential-expiry dedup keys embed the date, so the table
// grows without bound). Windows are AppSettings:BackgroundJobRunRetentionDays (default 90) and
// AppSettings:AlertEventRetentionDays (default 180). Platform-wide, so it keeps running for
// feature-disabled and suspended accounts.
public sealed class PlatformRetentionService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<PlatformRetentionService> logger) : BackgroundService
{
    private const int DefaultJobRunRetentionDays = 90;
    private const int DefaultAlertEventRetentionDays = 180;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(15);

    // Job keys whose Succeeded rows are permanent idempotency markers rather than run history:
    // workforce-expiration-scan keys on {qualificationId}:{threshold} and document-expiration on
    // {documentId}:{threshold}, neither of which carries a date. Deleting those rows would re-fire an
    // expiration alert the operator already received. Every other producer keys on a date, a timestamp
    // or a resource whose own state stops it being picked up again, so its history is safe to age out.
    //
    // A plain array: EF Core cannot translate IReadOnlySet<string>.Contains inside ExecuteDelete,
    // and the keys are written from these same constants so exact matching is correct.
    private static readonly string[] DurableMarkerJobKeys =
    [
        BackgroundJobKeys.WorkforceExpirationScan,
        BackgroundJobKeys.DocumentExpiration,
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await RunOnceAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "Platform retention cycle failed."); }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var now = DateTimeOffset.UtcNow;
        var jobRunsDeleted = await PurgeJobRunsAsync(context, now, cancellationToken);
        var alertEventsDeleted = await PurgeAlertEventsAsync(context, now, cancellationToken);

        if (jobRunsDeleted + alertEventsDeleted == 0)
        {
            return;
        }

        context.BackgroundJobRuns.Add(new BackgroundJobRun(
            BackgroundJobKeys.PlatformRetention, null, $"{jobRunsDeleted}/{alertEventsDeleted}", $"retention:{now:yyyyMMdd}", "Succeeded", 1, now)
        {
            CompletedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Platform retention deleted {JobRuns} job run(s) and {AlertEvents} resolved alert event(s).",
            jobRunsDeleted, alertEventsDeleted);
    }

    private async Task<int> PurgeJobRunsAsync(ApplicationDbContext context, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var retentionDays = configuration.GetValue<int?>("AppSettings:BackgroundJobRunRetentionDays") ?? DefaultJobRunRetentionDays;
        var cutoff = now.AddDays(-Math.Max(1, retentionDays));

        // The most recent row per JobKey is the /status page's whole data source (SVD-10/SVD-11): it is
        // preserved no matter how old it is, because for the on-work-only producers an old row IS the
        // healthy steady state and deleting it would blank the job out of the administrator's view.
        //
        // Two round trips rather than a GroupBy for the same reason BackgroundJobStatusReader gives: the
        // grouped form is not reliably translated by EF Core and the InMemory provider hides that.
        var jobKeys = await context.BackgroundJobRuns
            .Select(x => x.JobKey)
            .Distinct()
            .ToListAsync(cancellationToken);

        var latestIds = new List<Guid>(jobKeys.Count);
        foreach (var jobKey in jobKeys)
        {
            var latestId = await context.BackgroundJobRuns
                .Where(x => x.JobKey == jobKey)
                .OrderByDescending(x => x.StartedAt).ThenByDescending(x => x.BackgroundJobRunId)
                .Select(x => x.BackgroundJobRunId)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestId != Guid.Empty)
            {
                latestIds.Add(latestId);
            }
        }

        // Failed rows are left alone: they are the diagnostic trail for a job that stopped working.
        return await context.BackgroundJobRuns
            .Where(x => x.Status == "Succeeded"
                && x.StartedAt < cutoff
                && !DurableMarkerJobKeys.Contains(x.JobKey)
                && !latestIds.Contains(x.BackgroundJobRunId))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<int> PurgeAlertEventsAsync(ApplicationDbContext context, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var retentionDays = configuration.GetValue<int?>("AppSettings:AlertEventRetentionDays") ?? DefaultAlertEventRetentionDays;
        var cutoff = now.AddDays(-Math.Max(1, retentionDays));

        // Resolved only — an Open or Acknowledged event is still live work no matter its age. Events a
        // delivery row still points at are held back: AlertEventId is a plain column with no foreign key,
        // so nothing else would stop the notification feed from resolving to a row that no longer exists.
        return await context.AlertEvents
            .Where(x => x.Status == "Resolved"
                && x.LastSeenAt < cutoff
                && !context.NotificationDeliveries.Any(d => d.AlertEventId == x.AlertEventId))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
