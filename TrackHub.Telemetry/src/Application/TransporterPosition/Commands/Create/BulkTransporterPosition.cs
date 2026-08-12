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

namespace TrackHub.Telemetry.Application.TransporterPosition.Commands.Create;

// Gating matrix: the latest-position projection write is CORE — authorized only,
// never feature-gated. The live map must render for every authorized account regardless of the
// gps.positionHistory feature; only the *history* write (AppendPositionHistory*) is feature-gated.
//
// PrincipalTypes is the only tenant control this command can have: `Positions` is a collection and
// `TransporterPositionDto` carries no AccountId, so `RequestAccountResolver` finds no account (it
// never descends into a collection) and `AccountScopeBehavior` passes the request through untouched,
// while the writer upserts keyed on TransporterId alone. Restricting the caller to a service identity
// is what keeps a transporter id from being enough to write another account's position. Mirrors the
// gating on its twin, AppendPositionHistoryBatchCommand.
[Authorize(Resource = Resources.Positions, Action = Actions.Custom, PrincipalTypes = "ServiceClient")]
[AllowCrossAccount("Router/SyncWorker live-position feed: one global router_client/syncworker_client identity iterates every account and pushes that account's latest-position batch. The batch spans accounts by design and the token carries no account claim, so there is nothing to bind the request to. Declared here so the surface appears in the `grep -r AllowCrossAccount` inventory of deliberate cross-tenant writes.")]
public readonly record struct BulkTransporterPositionCommand(IEnumerable<TransporterPositionDto> Positions) : IRequest;

public class CreateTransporterCommandHandler(
    ITransporterPositionWriter writer) : IRequestHandler<BulkTransporterPositionCommand>
{
    public async Task Handle(BulkTransporterPositionCommand request, CancellationToken cancellationToken)
    {
        var positions = request.Positions as IList<TransporterPositionDto> ?? [.. request.Positions];

        // When several devices report for the same transporter in the same batch, keep only the
        // most recent position per TransporterId so the latest snapshot reflects the freshest fix.
        var grouped = positions.GroupBy(p => p.TransporterId).ToList();
        var deduplicated = grouped
            .Select(g => g.OrderByDescending(p => p.DeviceDateTime).First())
            .ToList();

        await writer.BulkTransporterPositionAsync(deduplicated, cancellationToken);
    }
}
