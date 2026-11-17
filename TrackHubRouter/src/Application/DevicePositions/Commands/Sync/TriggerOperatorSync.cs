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

using Common.Application.Attributes;
using Common.Application.Exceptions;
using Common.Domain.Constants;
using Microsoft.Extensions.Logging;
using TrackHub.Router.Domain.Exceptions;

namespace TrackHub.Router.Application.DevicePositions.Commands.Sync;

// Manager is the single entry for "sync now": it holds the user-facing grant check
// (SynchronizedDevices/Execute) and the per-operator throttle, then relays here under its global
// manager_client identity. Accepting a user token here would let a role without that grant, and
// without the throttle, reach the provider and reset the catalog.
[Authorize(Resource = Resources.SynchronizedDevices, Action = Actions.Execute, PrincipalTypes = "ServiceClient")]
[AllowCrossAccount("Manager relays a tenant's manual sync under its global manager_client identity, which carries no account claim; the operator row binds the account and the handler rejects a mismatch.")]
public readonly record struct TriggerOperatorSyncCommand(
    Guid AccountId,
    Guid OperatorId,
    string TriggerType = "MANUAL",
    string? CorrelationId = null,
    bool ResetDeviceCatalog = false,
    bool? AutoAssignNewDevices = null) : IRequest<bool>;

public class TriggerOperatorSyncCommandHandler(
    IOperatorSystemReader operatorSystemReader,
    ISyncDispatchQueue dispatchQueue,
    ILogger<TriggerOperatorSyncCommandHandler> logger) : IRequestHandler<TriggerOperatorSyncCommand, bool>
{
    public async Task<bool> Handle(TriggerOperatorSyncCommand request, CancellationToken cancellationToken)
    {
        // The manual-sync throttle lives at the single point of entry — Manager's
        // ManualSyncMinIntervalSeconds, which throws TooManyRequestsException
        // The Router's former hardcoded 5-minute cooldown is removed so a trigger accepted by
        // Manager can never be silently dropped here. The Router still validates operator/account/
        // enabled and returns typed errors instead of a silent false. One Manager read suffices:
        // Manager already validated the account (authorization + account-status gate) before
        // dispatching, and the operator row binds the account id. The caller is manager_client,
        // which holds no Operators grant, so the read runs under the Router's own service identity.
        var op = await operatorSystemReader.GetOperatorAsync(request.OperatorId, cancellationToken);
        if (op.AccountId != request.AccountId)
        {
            logger.LogWarning("Manual sync trigger rejected: operator {OperatorId} does not belong to account {AccountId}.", request.OperatorId, request.AccountId);
            throw new OperatorNotFoundException(request.OperatorId);
        }

        if (!op.Enabled)
        {
            logger.LogInformation("Manual sync trigger rejected: operator {OperatorId} is disabled.", request.OperatorId);
            throw new OperatorDisabledException(request.OperatorId);
        }

        logger.LogInformation(
            "Sync trigger accepted for operator {OperatorId} (account {AccountId}), trigger {TriggerType}, correlation {CorrelationId}.",
            request.OperatorId, request.AccountId, request.TriggerType, request.CorrelationId);

        // Accepted, not awaited: the provider read, the device write-back, the run record, the
        // health probe and the alerts all happen off the request. operator_sync_runs is where the
        // caller reads what actually happened.
        var accepted = dispatchQueue.TryEnqueue(new SyncDispatchRequest(
            op,
            request.TriggerType,
            request.CorrelationId,
            request.ResetDeviceCatalog,
            request.AutoAssignNewDevices ?? true));

        if (!accepted)
        {
            throw new TooManyRequestsException("Too many syncs are already queued. Try again shortly.");
        }

        return true;
    }
}
