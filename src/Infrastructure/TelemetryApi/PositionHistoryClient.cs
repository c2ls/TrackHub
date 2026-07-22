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

namespace TrackHub.TripManagement.Infrastructure.TelemetryApi;

/// <summary>
/// Route replay over Telemetry's <c>positionHistoryRange</c> under the service's own
/// <c>trip_client</c> identity. Telemetry validates a 31-day window and a 10 000-point cap and
/// rejects anything beyond them, so this client clamps to those limits before calling and reports
/// the clamp back as <see cref="RouteReplayVm.Truncated"/>: truncation is always explicit, never
/// a silently shortened route (spec 11 §7.5, acceptance 22).
/// </summary>
public class PositionHistoryClient(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Telemetry, asService: true)), IPositionHistoryClient
{
    internal const string PositionHistoryRangeQuery = @"
                query($accountId: UUID!, $transporterId: UUID!, $from: DateTime!, $to: DateTime!, $maxPoints: Int!) {
                    positionHistoryRange(query: { accountId: $accountId, transporterId: $transporterId, from: $from, to: $to, maxPoints: $maxPoints }) {
                        latitude
                        longitude
                        sourceTimestamp
                        speed
                    }
                }";

    /// <summary>Telemetry's <c>GetPositionHistoryRangeQueryValidator</c> maximum window.</summary>
    internal const int MaxRangeDays = 31;

    /// <summary>Telemetry's <c>GetPositionHistoryRangeQueryValidator</c> point cap.</summary>
    internal const int MaxPointsCap = 10000;

    public async Task<RouteReplayVm> GetRangeAsync(
        Guid accountId,
        Guid transporterId,
        DateTimeOffset from,
        DateTimeOffset to,
        int maxPoints,
        CancellationToken cancellationToken)
    {
        var effectiveMaxPoints = Math.Clamp(maxPoints, 1, MaxPointsCap);

        // The window is clamped from the END: a replay that cannot cover the whole request keeps
        // the most recent leg, and says so.
        var effectiveFrom = from;
        var windowClamped = false;
        if (to - from > TimeSpan.FromDays(MaxRangeDays))
        {
            effectiveFrom = to.AddDays(-MaxRangeDays);
            windowClamped = true;
        }

        var request = new GraphQLRequest
        {
            Query = PositionHistoryRangeQuery,
            Variables = new
            {
                accountId,
                transporterId,
                from = effectiveFrom,
                to,
                maxPoints = effectiveMaxPoints
            }
        };

        var history = await QueryAsync<IReadOnlyCollection<PositionHistoryResponse>>(request, cancellationToken);

        var points = history
            .Select(point => new RouteReplayPointVm(point.Latitude, point.Longitude, point.SourceTimestamp, point.Speed))
            .ToList();

        return new RouteReplayVm(points, windowClamped || points.Count >= effectiveMaxPoints);
    }
}

internal sealed record PositionHistoryResponse(double Latitude, double Longitude, DateTimeOffset SourceTimestamp, double? Speed);
