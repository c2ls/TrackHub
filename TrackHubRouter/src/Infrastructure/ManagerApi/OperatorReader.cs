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

namespace ManagerApi;

using GraphQL.Client.Abstractions;

// This class represents the implementation of the IOperatorReader interface
// It is responsible for reading operator data from the GraphQL service.
//
// Resolves operators under the CALLER's identity, so Manager applies that caller's account and
// group visibility. Credential material is redacted for callers without Credentials/Custom; use
// IOperatorSystemReader when the decrypted credential is required to reach a provider.
public class OperatorReader : GraphQLService, IOperatorReader
{
    public OperatorReader(IGraphQLClientFactory graphQLClient)
        : base(graphQLClient.CreateClient(Clients.Manager)) { }

    protected OperatorReader(IGraphQLClient graphQLClient) : base(graphQLClient) { }

    internal const string OperatorsByUserQuery = @"
                    query {
                        operatorsByUser
                        {
                            operatorId
                            protocolTypeId
                            accountId
                            enabled
                             syncIntervalMinutes
                             lastManualSyncAt
                             lastDeviceSyncAt
                             lastPositionSyncAt
                            healthStatus
                            lastHealthCheckAt
                            credential {
                                credentialId
                                uri
                                username
                                password
                                salt
                                key
                                key2
                                token
                                tokenExpiration
                                refreshToken
                                refreshTokenExpiration
                            }
                        }
                    }";

    internal const string OperatorByTransporterQuery = @"
                    query($transporterId: UUID!) {
                        operatorByTransporter(query: { transporterId: $transporterId })
                        {
                            operatorId
                            protocolTypeId
                            accountId
                            enabled
                             syncIntervalMinutes
                             lastManualSyncAt
                             lastDeviceSyncAt
                             lastPositionSyncAt
                            healthStatus
                            lastHealthCheckAt
                            credential {
                                credentialId
                                uri
                                username
                                password
                                salt
                                key
                                key2
                                token
                                tokenExpiration
                                refreshToken
                                refreshTokenExpiration
                            }
                        }
                    }";

    internal const string OperatorQuery = @"
                    query($id: UUID!) {
                        operator(query: { id: $id })
                        {
                            operatorId
                            protocolTypeId
                            accountId
                            enabled
                             syncIntervalMinutes
                             lastManualSyncAt
                             lastDeviceSyncAt
                             lastPositionSyncAt
                            healthStatus
                            lastHealthCheckAt
                            credential {
                                credentialId
                                uri
                                username
                                password
                                salt
                                key
                                key2
                                token
                                tokenExpiration
                                refreshToken
                                refreshTokenExpiration
                            }
                        }
                    }";

    // The master projection includes the credential: Manager only populates it for
    // service-client principals (the SyncWorker identity), and having it here means the worker
    // loops never re-fetch each operator individually just to obtain it.
    internal const string OperatorsMasterQuery = @"
            query($filter: FiltersInput!) {
                operatorsMaster(
                    query: { filter: $filter }
                    ) {
                        operatorId
                        protocolTypeId
                        accountId
                        enabled
                        syncIntervalMinutes
                        lastManualSyncAt
                        lastDeviceSyncAt
                        lastPositionSyncAt
                        healthStatus
                        lastHealthCheckAt
                        credential {
                            credentialId
                            uri
                            username
                            password
                            salt
                            key
                            key2
                            token
                            tokenExpiration
                            refreshToken
                            refreshTokenExpiration
                        }
                    }
            }";

    /// <summary>
    /// Retrieves a list of operators associated with the current user 
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation if needed</param>
    /// <returns>A collection of OperatorVm objects representing the operators</returns>
    public async Task<IEnumerable<OperatorVm>> GetOperatorsAsync(CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = OperatorsByUserQuery
        };
        return await QueryAsync<IEnumerable<OperatorVm>>(request, cancellationToken);
    }

    public async Task<OperatorVm> GetOperatorByTransporterAsync(Guid transporterId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = OperatorByTransporterQuery,
            Variables = new { transporterId }
        };
        return await QueryAsync<OperatorVm>(request, cancellationToken);
    }

    public async Task<OperatorVm> GetOperatorAsync(Guid operatorId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = OperatorQuery,
            Variables = new { id = operatorId }
        };
        return await QueryAsync<OperatorVm>(request, cancellationToken);
    }

    public async Task<IEnumerable<OperatorVm>> GetOperatorsByAccountsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = OperatorsMasterQuery,
            Variables = new
            {
                filter = new
                {
                    filters = new[]
                    {
                    new
                    {
                        key = "AccountId",
                        value = accountId
                    }
                }
                }
            }
        };
        return await QueryAsync<IEnumerable<OperatorVm>>(request, cancellationToken);
    }
}
