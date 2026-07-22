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

using Common.Application.Interfaces;

namespace TrackHub.Telemetry.Application.GpsIntegration.Queries;

// Replay read over stored history: ordered by SourceTimestamp, bounded by a maximum range and point
// cap (validated). User principals must have visibility over the transporter (privileged roles read
// account-wide) via the single visibility primitive; the Router service client reads on behalf of an
// already-authorized user. Account ownership is enforced by the reader's account filter.
[Authorize(Resource = Resources.PositionHistory, Action = Actions.Read, PrincipalTypes = "User,ServiceClient")]
[RequireFeature(FeatureKeys.GpsPositionHistory)]
// NOTE: this is the one marked request whose callers are MIXED. TripManagement's route-replay path
// (Infrastructure/TelemetryApi/PositionHistoryClient, asService: true) reads it under the global
// trip_client identity for whichever account owns the trip, which cannot satisfy the guard. The
// user-principal path is NOT left unguarded by this: the handler's own visibility gate still runs
// for PrincipalType.User, and the reader filters on AccountId regardless.
[AllowCrossAccount("TripManagement replays a trip's route under its global trip_client service identity, which has no account claim, for whichever account owns the trip. Users reaching the same query remain bound by the handler's per-user visibility gate.")]
public readonly record struct GetPositionHistoryRangeQuery(
    Guid AccountId,
    Guid TransporterId,
    DateTimeOffset From,
    DateTimeOffset To,
    int MaxPoints = 10000) : IRequest<IReadOnlyCollection<TransporterPositionHistoryVm>>;

public class GetPositionHistoryRangeQueryHandler(
    ITransporterPositionHistoryReader reader,
    IVisibleTransporterReader visibleReader,
    ICurrentPrincipal principal)
    : IRequestHandler<GetPositionHistoryRangeQuery, IReadOnlyCollection<TransporterPositionHistoryVm>>
{
    public async Task<IReadOnlyCollection<TransporterPositionHistoryVm>> Handle(GetPositionHistoryRangeQuery request, CancellationToken cancellationToken)
    {
        if (principal.PrincipalType == PrincipalType.User && principal.UserId.HasValue)
        {
            var visible = await visibleReader.GetVisibleTransporterIdsAsync(principal.UserId.Value, request.AccountId, cancellationToken);
            if (!visible.Contains(request.TransporterId))
            {
                throw new ForbiddenAccessException($"Transporter {request.TransporterId} is not visible to the requesting user.");
            }
        }

        return await reader.GetRangeAsync(request.AccountId, request.TransporterId, request.From, request.To, request.MaxPoints, cancellationToken);
    }
}
