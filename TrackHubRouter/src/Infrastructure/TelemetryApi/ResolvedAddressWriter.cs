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

public class ResolvedAddressWriter : GraphQLService, IResolvedAddressWriter
{
    public ResolvedAddressWriter(IGraphQLClientFactory graphQLClient) : base(graphQLClient.CreateClient(Clients.Telemetry)) { }

    protected ResolvedAddressWriter(IGraphQLClient graphQLClient) : base(graphQLClient) { }

    internal const string PersistResolvedAddressMutation = @"
                mutation($command: PersistResolvedAddressCommandInput!) {
                    persistResolvedAddress(command: $command)
                }";

    public async Task<bool> PersistResolvedAddressAsync(Guid? transporterPositionHistoryId, Guid? transporterId, AddressVm address, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = PersistResolvedAddressMutation,
            Variables = new
            {
                command = new
                {
                    transporterPositionHistoryId,
                    transporterId,
                    address = address.Address,
                    city = address.City,
                    state = address.State,
                    country = address.Country
                }
            }
        };

        return await MutationAsync<bool>(request, cancellationToken);
    }
}

// Persists with the Router's own service identity (never the user token), mirroring
// the PositionSystemWriter pattern.
public sealed class ResolvedAddressSystemWriter(IGraphQLClientFactory graphQLClient)
    : ResolvedAddressWriter(graphQLClient.CreateClient(Clients.Telemetry, asService: true)), IResolvedAddressSystemWriter;
