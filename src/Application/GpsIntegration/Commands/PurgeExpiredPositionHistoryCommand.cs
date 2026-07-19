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

using Microsoft.Extensions.Logging;

namespace TrackHub.Telemetry.Application.GpsIntegration.Commands;

// Retention purge: invoked per account by the Telemetry host's background
// retention loop. Telemetry does not own alert_events / audit_events (those stay in Manager), so
// this handler logs the outcome instead of writing cross-owner alert/audit rows.
[Authorize(Resource = Resources.PositionHistory, Action = Actions.Delete, PrincipalTypes = "ServiceClient")]
[RequireFeature(FeatureKeys.GpsPositionHistory, AllowGlobalServiceClient = false)]
public readonly record struct PurgeExpiredPositionHistoryCommand(Guid AccountId, DateTimeOffset Cutoff) : IRequest<int>;

public class PurgeExpiredPositionHistoryCommandHandler(
    ITransporterPositionHistoryWriter writer,
    ILogger<PurgeExpiredPositionHistoryCommandHandler> logger)
    : IRequestHandler<PurgeExpiredPositionHistoryCommand, int>
{
    public async Task<int> Handle(PurgeExpiredPositionHistoryCommand request, CancellationToken cancellationToken)
    {
        var purged = await writer.PurgeOlderThanAsync(request.AccountId, request.Cutoff, cancellationToken);
        logger.LogInformation(
            "Purged {Count} expired position-history row(s) for account {AccountId} older than {Cutoff:O}.",
            purged, request.AccountId, request.Cutoff);
        return purged;
    }
}
