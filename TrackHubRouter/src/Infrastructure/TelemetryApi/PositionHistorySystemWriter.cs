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

using System.Text.Json;

namespace TrackHub.Router.Infrastructure.TelemetryApi;

public class PositionHistorySystemWriter(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Telemetry, asService: true)), IPositionHistorySystemWriter
{
    internal const string AppendPositionHistoryBatchMutation = @"
                mutation($command: AppendPositionHistoryBatchCommandInput!) {
                    appendPositionHistoryBatch(command: $command)
                }";

    public async Task<int> AppendRangeAsync(Guid accountId, Guid operatorId, IEnumerable<PositionVm> positions, CancellationToken cancellationToken)
    {
        var rows = positions.Select(p => new
        {
            accountId,
            operatorId,
            // DeviceId is unknown in the position pipeline; the Telemetry service resolves it from the
            // transporter's active device assignment.
            deviceId = Guid.Empty,
            transporterId = p.TransporterId,
            sourceTimestamp = p.DeviceDateTime,
            latitude = p.Latitude,
            longitude = p.Longitude,
            altitude = p.Altitude,
            speed = p.Speed,
            course = p.Course,
            eventId = p.EventId,
            address = p.Address,
            city = p.City,
            state = p.State,
            country = p.Country,
            attributes = p.Attributes is null ? null : JsonSerializer.Serialize(p.Attributes),
            idempotencyKey = $"{operatorId}:{p.TransporterId}:{p.DeviceDateTime.ToUniversalTime():O}"
        }).ToArray();

        if (rows.Length == 0)
        {
            return 0;
        }

        var request = new GraphQLRequest
        {
            Query = AppendPositionHistoryBatchMutation,
            Variables = new
            {
                command = new
                {
                    accountId,
                    positions = rows
                }
            }
        };

        return await MutationAsync<int>(request, cancellationToken);
    }
}
