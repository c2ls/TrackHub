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

namespace TrackHub.Router.Infrastructure.TripApi;

public class TripPositionWriter(IGraphQLClientFactory graphQLClient) : GraphQLService(graphQLClient.CreateClient(Clients.TripManagement)), ITripPositionWriter
{
    // Single source of truth for the mutation this writer sends; the
    // ServiceContracts tests validate this exact string against the TripManagement schema.
    internal const string ProcessTripPositionsMutation = @"
                mutation($command: ProcessTripPositionsCommandInput!) {
                    processTripPositions(command: $command) {
                        processedCount
                        stopsArrived
                        stopsDeparted
                        deviationsRaised
                    }
                }";

    public async Task<TripProcessingResultVm> ProcessTripPositionsAsync(IEnumerable<PositionVm> positions, Guid accountId, CancellationToken token)
    {
        var request = new GraphQLRequest
        {
            Query = ProcessTripPositionsMutation,
            Variables = new
            {
                command = new
                {
                    accountId,
                    positions = positions.Select(p => new
                    {
                        transporterId = p.TransporterId,
                        longitude = p.Longitude,
                        latitude = p.Latitude,
                        deviceDateTime = p.DeviceDateTime
                    }).ToArray()
                }
            }
        };

        return await MutationAsync<TripProcessingResultVm>(request, token);
    }
}
