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
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Web.BackgroundServices;

// Delivery retention: daily, deletes delivered/failed/digested rows older than
// AppSettings:NotificationDeliveryRetentionDays (default 90). This cleanup keeps running for
// feature-disabled accounts.
public sealed class DeliveryRetentionService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<DeliveryRetentionService> logger) : BackgroundService
{
    private const string JobKey = BackgroundJobKeys.DeliveryRetention;
    private const int DefaultRetentionDays = 90;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await RunOnceAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "Delivery retention cycle failed."); }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var retentionDays = configuration.GetValue<int?>("AppSettings:NotificationDeliveryRetentionDays") ?? DefaultRetentionDays;
        var now = DateTimeOffset.UtcNow;
        var cutoff = now.AddDays(-Math.Max(1, retentionDays));

        var deleted = await context.NotificationDeliveries
            .Where(d => d.Created < cutoff
                && (d.Status == DeliveryStatuses.Sent || d.Status == DeliveryStatuses.Failed || d.Status == DeliveryStatuses.Digested))
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
        {
            context.BackgroundJobRuns.Add(new BackgroundJobRun(
                JobKey, null, deleted.ToString(), $"retention:{now:yyyyMMdd}", "Succeeded", 1, now)
            {
                CompletedAt = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Delivery retention deleted {Count} delivery row(s) older than {Days} day(s).", deleted, retentionDays);
        }
    }
}
