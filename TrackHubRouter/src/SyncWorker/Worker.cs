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

using Common.Mediator;
using TrackHub.Router.Application.DevicePositions.Commands.Health;
using TrackHub.Router.Application.DevicePositions.Commands.Sync;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;

namespace TrackHub.Router.SyncWorker;

public class Worker(ILogger<Worker> logger, IServiceProvider serviceProvider) : BackgroundService
{
    private static readonly TimeSpan PositionInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DeviceSyncCheckInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan HealthCheckInterval = TimeSpan.FromMinutes(1);

    private readonly ILogger<Worker> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var positionLoop = RunLoopAsync("position-sync", PositionInterval, RunPositionSyncAsync, stoppingToken);
        var deviceLoop = RunLoopAsync("device-sync", DeviceSyncCheckInterval, RunDeviceSyncAsync, stoppingToken);
        var healthLoop = RunLoopAsync("operator-health", HealthCheckInterval, RunHealthCheckAsync, stoppingToken);

        await Task.WhenAll(positionLoop, deviceLoop, healthLoop);
    }

    private async Task RunLoopAsync(string name, TimeSpan interval, Func<CancellationToken, Task> action, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Worker loop {Loop} tick at {Time}.", name, DateTimeOffset.UtcNow);
                await action(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker loop {Loop} iteration failed.", name);
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task RunPositionSyncAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new SyncPositionCommand(), stoppingToken);
    }

    private async Task RunDeviceSyncAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var accountReader = scope.ServiceProvider.GetRequiredService<IAccountReader>();
        var operatorReader = scope.ServiceProvider.GetRequiredService<IOperatorReader>();
        var backoff = scope.ServiceProvider.GetRequiredService<IOperatorSyncBackoff>();

        var accounts = await accountReader.GetAccountsToSyncAsync(stoppingToken);
        var now = DateTimeOffset.UtcNow;

        foreach (var account in accounts.Where(a => a.GpsIntegrationEnabled))
        {
            IEnumerable<Domain.Models.OperatorVm> operators;
            try
            {
                operators = await operatorReader.GetOperatorsByAccountsAsync(account.AccountId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load operators for account {AccountId}.", account.AccountId);
                continue;
            }

            foreach (var op in operators.Where(o => o.Enabled))
            {
                var intervalMinutes = Math.Max(1, op.SyncIntervalMinutes);
                // Gate on persisted LastDeviceSyncAt so multiple Router instances
                // do not duplicate scheduled syncs.
                var last = op.LastDeviceSyncAt ?? DateTimeOffset.MinValue;

                if (now - last < TimeSpan.FromMinutes(intervalMinutes))
                {
                    continue;
                }

                // The master projection already carries the credential under the worker's
                // service identity — no per-operator re-fetch.
                if (op.Credential is null)
                {
                    continue;
                }

                // Skip operators still inside their failure backoff window so a persistently
                // failing operator is not re-attempted at full cadence (router-audit A-15).
                if (backoff.IsInBackoff(op.OperatorId, now))
                {
                    continue;
                }

                try
                {
                    var succeeded = await sender.Send(new SyncOperatorDevicesCommand(op, "AUTOMATIC"), stoppingToken);
                    if (succeeded)
                    {
                        backoff.RecordSuccess(op.OperatorId);
                    }
                    else
                    {
                        backoff.RecordFailure(op.OperatorId, DateTimeOffset.UtcNow);
                    }
                }
                catch (Exception ex)
                {
                    backoff.RecordFailure(op.OperatorId, DateTimeOffset.UtcNow);
                    _logger.LogError(ex, "Scheduled device sync failed for operator {OperatorId}.", op.OperatorId);
                }
            }
        }
    }

    private async Task RunHealthCheckAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var accountReader = scope.ServiceProvider.GetRequiredService<IAccountReader>();
        var operatorReader = scope.ServiceProvider.GetRequiredService<IOperatorReader>();
        var backoff = scope.ServiceProvider.GetRequiredService<IOperatorSyncBackoff>();

        var accounts = await accountReader.GetAccountsToSyncAsync(stoppingToken);
        var now = DateTimeOffset.UtcNow;

        // Operator health monitoring is core behavior for every account with provider
        // integration running in the background; it is not a separately billed feature.
        foreach (var account in accounts.Where(a => a.GpsIntegrationEnabled))
        {
            IEnumerable<Domain.Models.OperatorVm> operators;
            try
            {
                operators = await operatorReader.GetOperatorsByAccountsAsync(account.AccountId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load operators for health check on account {AccountId}.", account.AccountId);
                continue;
            }

            foreach (var op in operators.Where(o => o.Enabled))
            {
                // Gate on persisted LastHealthCheckAt so horizontally-scaled
                // Router instances do not duplicate health checks.
                var last = op.LastHealthCheckAt ?? DateTimeOffset.MinValue;
                if (now - last < HealthCheckInterval)
                {
                    continue;
                }

                // Credential comes with the master projection (service identity) — no re-fetch.
                if (op.Credential is null)
                {
                    continue;
                }

                // Persistently unreachable operators back off (router-audit A-15): the offline
                // alert is already deduped, so full-cadence probing only produced log spam; a
                // recovered operator's first successful probe (within the backoff window) clears it.
                if (backoff.IsInBackoff(op.OperatorId, now))
                {
                    continue;
                }

                try
                {
                    var healthy = await sender.Send(new RecordOperatorHealthCommand(op), stoppingToken);
                    if (healthy)
                    {
                        backoff.RecordSuccess(op.OperatorId);
                    }
                    else
                    {
                        backoff.RecordFailure(op.OperatorId, DateTimeOffset.UtcNow);
                    }
                }
                catch (Exception ex)
                {
                    backoff.RecordFailure(op.OperatorId, DateTimeOffset.UtcNow);
                    _logger.LogError(ex, "Operator {OperatorId} health check failed.", op.OperatorId);
                }
            }
        }
    }
}

