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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrackHub.Router.Application.DevicePositions.Events;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Application.DevicePositions.Commands.Sync;

public readonly record struct SyncPositionCommand() : IRequest<bool>;

public class UpdateTransporterCommandHandler(IAccountReader reader,
    IOperatorReader operatorReader,
    IExecutionIntervalManager intervalManager,
    IPublisher publisher,
    IConfiguration configuration,
    ILogger<UpdateTransporterCommandHandler> logger) : IRequestHandler<SyncPositionCommand, bool>
{
    // Global cap on concurrent per-operator provider fetches across ALL accounts in one cycle, so
    // account-level parallelism can never overwhelm the Router or the providers (router-audit A-11).
    // Configurable; defaults to 10 — the previous per-account fan-out width — so load is unchanged
    // until explicitly raised.
    private readonly int _maxConcurrentOperatorSyncs =
        int.TryParse(configuration["AppSettings:MaxConcurrentOperatorSyncs"], out var configured) && configured > 0
            ? configured
            : 10;

    public async Task<bool> Handle(SyncPositionCommand request, CancellationToken cancellationToken)
    {
        var accounts = await reader.GetAccountsToSyncAsync(cancellationToken);
        var dueAccounts = accounts
            .Where(account => account.GpsIntegrationEnabled && intervalManager.ShouldExecuteTask(account))
            .ToArray();
        if (dueAccounts.Length == 0)
        {
            return true;
        }

        // Accounts are processed concurrently (no head-of-line blocking — one slow account no longer
        // stalls the rest), but every operator fan-out draws from a single shared gate so the total
        // in-flight provider syncs stay bounded fleet-wide (router-audit A-11).
        using var gate = new SemaphoreSlim(_maxConcurrentOperatorSyncs);
        await Task.WhenAll(dueAccounts.Select(account => ProcessAccountAsync(account, gate, cancellationToken)));
        return true;
    }

    private async Task ProcessAccountAsync(AccountSettingsVm account, SemaphoreSlim gate, CancellationToken cancellationToken)
    {
        IEnumerable<OperatorVm> operators;
        try
        {
            operators = await operatorReader.GetOperatorsByAccountsAsync(account.AccountId, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Could not even load operators — do NOT advance the interval; retry next tick.
            logger.LogError(ex, "Failed to load operators for position sync on account {AccountId}.", account.AccountId);
            return;
        }

        // The master projection already carries each operator's credential under the worker's
        // service identity — no per-operator re-fetch.
        var tasks = operators
            .Where(o => o.Enabled && o.Credential is not null)
            .Select(async @operator =>
            {
                await gate.WaitAsync(cancellationToken);
                try
                {
                    await publisher.Publish(new OperatorRetrieved.Notification(@operator, account), cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Isolate per-operator failures: one operator (e.g. bad credentials, an
                    // unregistered protocol) must not abort the account's fan-out nor skip the
                    // interval update below (router-audit A-05).
                    logger.LogError(ex, "Position sync failed for operator {OperatorId} (account {AccountId}).",
                        @operator.OperatorId, account.AccountId);
                }
                finally
                {
                    gate.Release();
                }
            });
        await Task.WhenAll(tasks);
        // Always advance the interval: individual operator failures are handled above, so a single
        // poison-pill operator cannot pin the account to a tight retry loop.
        intervalManager.UpdateLastExecutionTime(account.AccountId);
    }
}
