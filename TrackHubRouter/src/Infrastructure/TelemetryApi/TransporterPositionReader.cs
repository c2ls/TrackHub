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

public class TransporterPositionReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Telemetry)), ITransporterPositionReader
{
    internal const string TransporterPositionByOperatorQuery = @"
                query($operatorId: UUID!) {
                    transporterPositionByOperator(query: { operatorId: $operatorId })
                    {
                        transporterId
                        transporterType
                        state
                        speed
                        longitude
                        latitude
                        geometryId
                        eventId
                        deviceName
                        deviceDateTime
                        course
                        country
                        city
                        attributes {
                            temperature
                            satellites
                            mileage
                            ignition
                            hourmeter
                            extra
                        }
                        altitude
                        address
                    }
                }";

    // Batched live-map read: one Telemetry round trip for all the caller's stored-projection
    // operators instead of one call per operator per map refresh.
    internal const string TransporterPositionsByOperatorsQuery = @"
                query($operatorIds: [UUID!]!) {
                    transporterPositionsByOperators(operatorIds: $operatorIds)
                    {
                        transporterId
                        transporterType
                        state
                        speed
                        longitude
                        latitude
                        geometryId
                        eventId
                        deviceName
                        deviceDateTime
                        course
                        country
                        city
                        attributes {
                            temperature
                            satellites
                            mileage
                            ignition
                            hourmeter
                            extra
                        }
                        altitude
                        address
                    }
                }";

    public async Task<IEnumerable<PositionVm>> GetTransporterPositionAsync(Guid operatorId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = TransporterPositionByOperatorQuery,
            Variables = new { operatorId }
        };
        return await QueryAsync<IEnumerable<PositionVm>>(request, cancellationToken);
    }

    public async Task<IEnumerable<PositionVm>> GetTransporterPositionsAsync(IReadOnlyCollection<Guid> operatorIds, CancellationToken cancellationToken)
    {
        if (operatorIds.Count == 0)
        {
            return [];
        }

        var request = new GraphQLRequest
        {
            Query = TransporterPositionsByOperatorsQuery,
            Variables = new { operatorIds }
        };
        return await QueryAsync<IEnumerable<PositionVm>>(request, cancellationToken);
    }

    public async Task<PositionVm?> GetTransporterPositionAsync(Guid operatorId, Guid transporterId, CancellationToken cancellationToken)
    {
        var positions = await GetTransporterPositionAsync(operatorId, cancellationToken);
        return positions
            .Where(p => p.TransporterId == transporterId)
            .Select(p => (PositionVm?)p)
            .FirstOrDefault();
    }
}
