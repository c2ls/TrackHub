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

using TrackHub.TripManagement.Application.Trips.Services.Interfaces;

namespace TrackHub.TripManagement.Web.BackgroundServices;

/// <summary>
/// Every 15 minutes, raises <c>TripStartDue</c> once for each <c>Created</c> trip whose planned
/// start falls inside its account's lead window.
/// <para>
/// <b>It never changes trip status.</b> Auto-starting a trip nobody began would stamp an
/// <c>ActualStartAt</c> and a distance baseline for a vehicle that has not moved, and every report
/// downstream would inherit the fiction. A reminder is a nudge to a human, not a state transition
/// (spec 11 §10).
/// </para>
/// <para>
/// <b>On-work-only recorder (SVD-11):</b> a row is written ONLY when a reminder was actually
/// raised, so an old <c>trip-schedule-reminder</c> timestamp is the healthy steady state and must
/// render neutrally on <c>/status</c>.
/// </para>
/// </summary>
public sealed class TripScheduleReminderService(
    IServiceScopeFactory scopeFactory,
    ILogger<TripScheduleReminderService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(90);

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
                logger.LogError(ex, "Trip schedule reminder cycle failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    // internal, not private, ONLY so the cycle can be driven directly by Web.UnitTests: ExecuteAsync
    // is wrapped in a 45/90 s startup delay and an infinite loop, so the on-work-only rule (SVD-11)
    // is otherwise unreachable from a test. Behaviour is unchanged.
    internal async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;

        using var scope = scopeFactory.CreateScope();
        var etaService = scope.ServiceProvider.GetRequiredService<ITripEtaService>();
        var raised = await etaService.RaiseStartRemindersAsync(cancellationToken);

        if (raised == 0)
            return;

        logger.LogInformation("Trip schedule reminder raised {Raised} start-due reminder(s)", raised);

        try
        {
            var recorder = scope.ServiceProvider.GetRequiredService<IBackgroundJobRunRecorder>();
            await recorder.RecordAsync(
                BackgroundJobKeys.TripScheduleReminder,
                null,
                raised.ToString(),
                $"trip-schedule-reminder:{startedAt:yyyyMMddHHmmssfff}",
                "Succeeded",
                startedAt,
                DateTimeOffset.UtcNow,
                null,
                null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to record background job run for {JobKey}", BackgroundJobKeys.TripScheduleReminder);
        }
    }
}
