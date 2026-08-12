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
using Common.Domain.Constants;
using Microsoft.Extensions.Logging;
using TrackHub.Router.Domain.Exceptions;

namespace TrackHub.Router.Application.DevicePositions.Commands.Sync;

// Manual sync is an expected user feature: it is authorized with the same Credentials/Custom grant
// that portal roles hold for PingOperator (NOT restricted to service clients), so users keep the
// "sync now" capability while the pipeline enforces authentication + permission instead of relying
// solely on Manager's upstream check. Rate-limited because each accepted trigger reaches the
// external provider on demand.
// Operators/Custom — see PingOperator. Operating an integration does not require credential-viewing
// permission; the Router fetches the credential with its own service identity.
[Authorize(Resource = Resources.Operators, Action = Actions.Custom)]
[RateLimiting(PermitLimit = 6, WindowSeconds = 60)]
public readonly record struct TriggerOperatorSyncCommand(
    Guid AccountId,
    Guid OperatorId,
    string TriggerType = "MANUAL",
    string? CorrelationId = null,
    bool ResetDeviceCatalog = false,
    bool? AutoAssignNewDevices = null) : IRequest<bool>;

public class TriggerOperatorSyncCommandHandler(
    IOperatorReader operatorReader,
    IOperatorSystemReader operatorSystemReader,
    ISender sender,
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
        // dispatching, and the operator row binds the account id.
        var op = await operatorReader.GetOperatorAsync(request.OperatorId, cancellationToken);
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

        // The account/enabled checks above run on the caller-scoped read. Re-read with the Router's
        // service identity so the device sync receives the decrypted credential.
        var authorized = await operatorSystemReader.GetOperatorAsync(op.OperatorId, cancellationToken);

        return await sender.Send(new SyncOperatorDevicesCommand(
            authorized,
            request.TriggerType,
            request.CorrelationId,
            request.ResetDeviceCatalog,
            request.AutoAssignNewDevices ?? true), cancellationToken);
    }
}
