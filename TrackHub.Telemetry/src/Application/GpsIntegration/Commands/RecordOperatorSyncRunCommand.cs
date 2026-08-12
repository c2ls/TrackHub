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

namespace TrackHub.Telemetry.Application.GpsIntegration.Commands;

// The handler no longer writes the denormalized operator sync-summary
// columns (Telemetry has read-only access to the operator master row); the summary is derived from
// the telemetry tables at read time.
// The account is nested in OperatorSyncRunDto. The User half of PrincipalTypes is the manual
// "sync now" path, which reaches here through Router's TriggerOperatorSyncCommand — that command
// carries a TOP-LEVEL AccountId (still guarded) and its handler rejects an operator belonging to a
// different account before any run is recorded, so the opt-out does not leave the user path open.
[Authorize(Resource = Resources.OperatorSyncRuns, Action = Actions.Write, PrincipalTypes = "User,ServiceClient")]
[AllowCrossAccount("Router/SyncWorker device-sync loop: one global router_client/syncworker_client identity syncs every account's operators and records each run for whichever account owns the operator. The token carries no account claim.")]
public readonly record struct RecordOperatorSyncRunCommand(OperatorSyncRunDto Run) : IRequest<OperatorSyncRunVm>;

public class RecordOperatorSyncRunCommandHandler(IOperatorSyncRunWriter writer)
    : IRequestHandler<RecordOperatorSyncRunCommand, OperatorSyncRunVm>
{
    public async Task<OperatorSyncRunVm> Handle(RecordOperatorSyncRunCommand request, CancellationToken cancellationToken)
        => await writer.RecordAsync(request.Run, cancellationToken);
}
