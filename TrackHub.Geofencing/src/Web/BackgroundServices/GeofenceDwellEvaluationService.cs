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

using TrackHub.Geofencing.Application.GeofenceEvents.Services.Interfaces;

namespace TrackHub.Geofencing.Web.BackgroundServices;

/// <summary>
/// Hosted loop for dwell-threshold evaluation: dwell alerts are triggered by
/// elapsed time, not by new positions, so they cannot ride the SyncWorker-driven detection path.
/// Scans the open-visit partial index every cycle via <see cref="IDwellEvaluationService"/>.
/// </summary>
public sealed class GeofenceDwellEvaluationService(
    IServiceScopeFactory scopeFactory,
    ILogger<GeofenceDwellEvaluationService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(StartupDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Geofence dwell evaluation cycle failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var evaluationService = scope.ServiceProvider.GetRequiredService<IDwellEvaluationService>();
        var alerted = await evaluationService.EvaluateDwellAsync(cancellationToken);
        if (alerted > 0)
            logger.LogInformation("Geofence dwell evaluation emitted {Alerted} alert(s)", alerted);
    }
}
