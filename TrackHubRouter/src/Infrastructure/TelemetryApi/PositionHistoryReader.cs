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

namespace TrackHub.Router.Infrastructure.TelemetryApi;

// STORED replay source read. Uses the user-token Telemetry client so the Telemetry service enforces
// the gps.positionHistory flag, PositionHistory authorization, and group visibility.
public class PositionHistoryReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Telemetry)), IPositionHistoryReader
{
    private const int MaxPoints = 10000;

    internal const string PositionHistoryRangeQuery = @"
                query($accountId: UUID!, $transporterId: UUID!, $from: DateTime!, $to: DateTime!, $maxPoints: Int!) {
                    positionHistoryRange(query: { accountId: $accountId, transporterId: $transporterId, from: $from, to: $to, maxPoints: $maxPoints })
                    {
                        sourceTimestamp
                        latitude
                        longitude
                        altitude
                        speed
                        course
                        eventId
                        address
                        city
                        state
                        country
                        transporterId
                    }
                }";

    public async Task<IEnumerable<PositionVm>> GetPositionHistoryRangeAsync(Guid accountId, Guid transporterId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = PositionHistoryRangeQuery,
            Variables = new { accountId, transporterId, from, to, maxPoints = MaxPoints }
        };

        var rows = await QueryAsync<IEnumerable<PositionHistoryRow>>(request, cancellationToken);
        return (rows ?? []).Select(row => new PositionVm(
            row.TransporterId,
            string.Empty,
            string.Empty,
            row.Latitude,
            row.Longitude,
            row.Altitude,
            row.SourceTimestamp,
            null,
            row.Speed,
            row.Course,
            row.EventId,
            row.Address,
            row.City,
            row.State,
            row.Country,
            null));
    }

    private readonly record struct PositionHistoryRow(
        Guid TransporterId,
        DateTimeOffset SourceTimestamp,
        double Latitude,
        double Longitude,
        double? Altitude,
        double Speed,
        double? Course,
        int? EventId,
        string? Address,
        string? City,
        string? State,
        string? Country);
}
