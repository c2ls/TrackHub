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
using Microsoft.EntityFrameworkCore;
using TrackHub.Telemetry.Infrastructure.TelemetryDB;

namespace TrackHub.Telemetry.Web.BackgroundServices;

// Retention purge loop: once per day, for each account with gps.positionHistory
// enabled, deletes transporter_position_history rows older than the account's retentionDays. Runs as
// a host-internal job against the Telemetry-owned schema directly (no per-account principal, no
// cross-owner alert/audit writes), replacing the Manager-hosted trigger.
public sealed class PositionRetentionPurgeService(
    IServiceScopeFactory scopeFactory,
    ILogger<PositionRetentionPurgeService> logger) : BackgroundService
{
    private static readonly string PositionHistoryFeatureKey = Common.Domain.Constants.FeatureKeys.GpsPositionHistory;
    private const int DefaultRetentionDays = 30;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
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
                logger.LogError(ex, "Position-history retention purge cycle failed.");
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

        var features = await context.AccountFeatures
            .Where(f => f.FeatureKey == PositionHistoryFeatureKey && f.Enabled)
            .Select(f => new { f.AccountId, f.ConfigurationJson })
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var totalPurged = 0;

        foreach (var feature in features)
        {
            var retentionDays = ResolveRetentionDays(feature.ConfigurationJson);
            var cutoff = now.AddDays(-retentionDays);
            try
            {
                var purged = await context.TransporterPositionHistory
                    .Where(x => x.AccountId == feature.AccountId && x.SourceTimestamp < cutoff)
                    .ExecuteDeleteAsync(cancellationToken);
                totalPurged += purged;
                if (purged > 0)
                {
                    logger.LogInformation(
                        "Retention purge removed {Count} position-history row(s) for account {AccountId} older than {Cutoff:O} ({RetentionDays}d).",
                        purged, feature.AccountId, cutoff, retentionDays);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Retention purge failed for account {AccountId}.", feature.AccountId);
            }
        }

        logger.LogInformation("Retention purge cycle complete: {Accounts} account(s) processed, {Total} row(s) removed.", features.Count, totalPurged);
    }

    private static int ResolveRetentionDays(string? configurationJson)
    {
        if (string.IsNullOrWhiteSpace(configurationJson))
        {
            return DefaultRetentionDays;
        }

        try
        {
            using var doc = JsonDocument.Parse(configurationJson);
            return doc.RootElement.TryGetProperty("retentionDays", out var rd) && rd.TryGetInt32(out var days) && days > 0
                ? days
                : DefaultRetentionDays;
        }
        catch (JsonException)
        {
            return DefaultRetentionDays;
        }
    }
}
