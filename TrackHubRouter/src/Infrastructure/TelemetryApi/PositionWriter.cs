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

using GraphQL.Client.Abstractions;

namespace TrackHub.Router.Infrastructure.TelemetryApi;

public class PositionWriter : GraphQLService, IPositionWriter
{
    public PositionWriter(IGraphQLClientFactory graphQLClient) : base(graphQLClient.CreateClient(Clients.Telemetry)) { }

    protected PositionWriter(IGraphQLClient graphQLClient) : base(graphQLClient) { }

    internal const string BulkTransporterPositionMutation = @"
                mutation($command: BulkTransporterPositionCommandInput!) {
                    bulkTransporterPosition(command: $command)
                }";

    public async Task<bool> AddOrUpdatePositionAsync(IEnumerable<PositionVm> positions, CancellationToken token)
    {
        var request = new GraphQLRequest
        {
            Query = BulkTransporterPositionMutation,
            Variables = new
            {
                command = new
                {
                    positions = positions.Select(p => new
                    {
                        transporterId = p.TransporterId,
                        speed = p.Speed,
                        longitude = p.Longitude,
                        state = p.State,
                        latitude = p.Latitude,
                        eventId = p.EventId,
                        deviceDateTime = p.DeviceDateTime,
                        course = p.Course,
                        country = p.Country,
                        city = p.City,
                        altitude = p.Altitude,
                        address = p.Address,
                        // AttributesVm now carries the open Extra bag (a JSON string) alongside the
                        // 5 promoted fields; it maps 1:1 onto Telemetry's AttributesDtoInput
                        // (extra: String) with no explicit projection needed (router-audit A-03).
                        attributes = p.Attributes
                    }).ToArray()
                }
            }
        };

        return await MutationAsync<bool>(request, token);
    }
}
