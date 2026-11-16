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
using TrackHub.Router.Application.Sync;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Enumerators;
using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.SyncWorker;

/// <summary>
/// Walks every GPS-enabled account and dispatches the due operators. Accounts run concurrently
/// and every operator draws from one shared gate: sequentially, a single unreachable provider
/// held the whole platform's cadence for its 30 s timeout, one operator at a time, so healthy
/// accounts missed their cycle entirely.
/// </summary>
public sealed class OperatorFanOut(
    IServiceScopeFactory scopeFactory,
    IAccountReader accountReader,
    IOperatorReader operatorReader,
    IConfiguration configuration,
    ILogger<OperatorFanOut> logger)
{
    public async Task RunAsync(
        string loopName,
        DateTimeOffset now,
        Func<OperatorVm, DateTimeOffset, bool> isDue,
        Func<ISender, OperatorVm, CancellationToken, Task<OperatorRunOutcome>> dispatch,
        CancellationToken cancellationToken)
    {
        var accounts = await accountReader.GetAccountsToSyncAsync(cancellationToken);

        using var gate = new SemaphoreSlim(OperatorSyncConcurrency.Resolve(configuration));
        await Task.WhenAll(accounts
            .Where(account => account.GpsIntegrationEnabled)
            .Select(account => ProcessAccountOperatorsAsync(loopName, account.AccountId, gate, now, isDue, dispatch, cancellationToken)));
    }

    private async Task ProcessAccountOperatorsAsync(
        string loopName,
        Guid accountId,
        SemaphoreSlim gate,
        DateTimeOffset now,
        Func<OperatorVm, DateTimeOffset, bool> isDue,
        Func<ISender, OperatorVm, CancellationToken, Task<OperatorRunOutcome>> dispatch,
        CancellationToken cancellationToken)
    {
        IEnumerable<OperatorVm> operators;
        try
        {
            operators = await operatorReader.GetOperatorsByAccountsAsync(accountId, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Loop {Loop} failed to load operators for account {AccountId}.", loopName, accountId);
            return;
        }

        var due = operators
            // The master projection already carries the credential under the worker's service
            // identity, and gating on the persisted timestamp keeps horizontally-scaled Router
            // instances from duplicating work. Operators inside their failure backoff window are
            // skipped so a persistently failing provider is not re-attempted at full cadence.
            .Where(op => op.Enabled && op.Credential is not null && isDue(op, now) && !OperatorSyncBackoffPolicy.IsInBackoff(op, now))
            .Select(op => DispatchOperatorAsync(loopName, accountId, op, gate, dispatch, cancellationToken));
        await Task.WhenAll(due);
    }

    private async Task DispatchOperatorAsync(
        string loopName,
        Guid accountId,
        OperatorVm @operator,
        SemaphoreSlim gate,
        Func<ISender, OperatorVm, CancellationToken, Task<OperatorRunOutcome>> dispatch,
        CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            // A scope per dispatch: the fan-out is concurrent, and a handler's scoped dependencies
            // must not be shared across parallel sends.
            using var scope = scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var backoff = scope.ServiceProvider.GetRequiredService<IOperatorSyncBackoff>();

            OperatorRunOutcome outcome;
            try
            {
                outcome = await dispatch(sender, @operator, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                outcome = OperatorRunOutcome.Failed;
                logger.LogError(ex, "Loop {Loop} failed for operator {OperatorId} (account {AccountId}).",
                    loopName, @operator.OperatorId, accountId);
            }

            if (outcome == OperatorRunOutcome.Succeeded)
            {
                await backoff.RecordSuccessAsync(@operator, cancellationToken);
            }
            else if (outcome == OperatorRunOutcome.Failed)
            {
                await backoff.RecordFailureAsync(@operator, DateTimeOffset.UtcNow, cancellationToken);
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Loop {Loop} could not record the backoff state of operator {OperatorId} (account {AccountId}).",
                loopName, @operator.OperatorId, accountId);
        }
        finally
        {
            gate.Release();
        }
    }
}
