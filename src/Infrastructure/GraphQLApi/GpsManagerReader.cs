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

using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models.Manager;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

// Feeds seven GPS reports. Each Manager read below is paged at the source and each report needs the
// complete set, so every one drains its pages rather than taking the first.
public class GpsManagerReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IGpsManagerReader
{
    // Single source of truth for the queries this reader sends; the
    // ServiceContracts tests validate these exact strings against the Manager schema.
    internal const string OperatorsByCurrentAccountQuery = @"
                query($skip: Int!, $take: Int!) {
                    operatorsByCurrentAccount(query: { skip: $skip, take: $take }) {
                        items {
                            operatorId
                            name
                            enabled
                        }
                        totalCount
                    }
                }";

    internal const string SynchronizedDevicesQuery = @"
                query($accountId: UUID!, $detectedStatus: DetectedStatus, $operatorId: UUID, $skip: Int!, $take: Int!) {
                    synchronizedDevices(query: { accountId: $accountId, detectedStatus: $detectedStatus, operatorId: $operatorId, skip: $skip, take: $take }) {
                        items {
                            deviceId
                            accountId
                            operatorId
                            serial
                            name
                            identifier
                            providerDisplayName
                            providerStatus
                            detectedStatus
                            firstSeenAt
                            lastSeenAt
                            lastAssignedAt
                            ignoredAt
                        }
                        totalCount
                    }
                }";

    internal const string UnassignedSynchronizedDevicesQuery = @"
                query($accountId: UUID!, $skip: Int!, $take: Int!) {
                    unassignedSynchronizedDevices(query: { accountId: $accountId, skip: $skip, take: $take }) {
                        items {
                            deviceId
                            accountId
                            operatorId
                            serial
                            name
                            identifier
                            providerDisplayName
                            providerStatus
                            detectedStatus
                            firstSeenAt
                            lastSeenAt
                            lastAssignedAt
                            ignoredAt
                        }
                        totalCount
                    }
                }";

    internal const string TransporterDeviceAssignmentsByAccountQuery = @"
                query($accountId: UUID!, $activeOnly: Boolean!, $skip: Int!, $take: Int!) {
                    transporterDeviceAssignmentsByAccount(query: { accountId: $accountId, activeOnly: $activeOnly, skip: $skip, take: $take }) {
                        items {
                            transporterDeviceAssignmentId
                            accountId
                            transporterId
                            deviceId
                            effectiveFrom
                            effectiveTo
                            priority
                            isPrimary
                            status
                            assignmentReason
                        }
                        totalCount
                    }
                }";

    public Task<IReadOnlyCollection<ManagerOperatorVm>> GetOperatorsAsync(CancellationToken cancellationToken)
        => ManagerPageDrain.FetchAllAsync<ManagerOperatorVm>(
            (skip, take) => new GraphQLRequest { Query = OperatorsByCurrentAccountQuery, Variables = new { skip, take } },
            (request, token) => QueryAsync<ManagerPage<ManagerOperatorVm>>(request, token),
            "operatorsByCurrentAccount",
            cancellationToken);

    public Task<IReadOnlyCollection<ManagerDeviceVm>> GetSynchronizedDevicesAsync(Guid accountId, string? detectedStatus, Guid? operatorId, CancellationToken cancellationToken)
        => ManagerPageDrain.FetchAllAsync<ManagerDeviceVm>(
            (skip, take) => new GraphQLRequest
            {
                Query = SynchronizedDevicesQuery,
                Variables = new { accountId, detectedStatus, operatorId, skip, take }
            },
            (request, token) => QueryAsync<ManagerPage<ManagerDeviceVm>>(request, token),
            "synchronizedDevices",
            cancellationToken);

    public Task<IReadOnlyCollection<ManagerDeviceVm>> GetUnassignedDevicesAsync(Guid accountId, CancellationToken cancellationToken)
        => ManagerPageDrain.FetchAllAsync<ManagerDeviceVm>(
            (skip, take) => new GraphQLRequest
            {
                Query = UnassignedSynchronizedDevicesQuery,
                Variables = new { accountId, skip, take }
            },
            (request, token) => QueryAsync<ManagerPage<ManagerDeviceVm>>(request, token),
            "unassignedSynchronizedDevices",
            cancellationToken);

    public Task<IReadOnlyCollection<ManagerTransporterDeviceAssignmentVm>> GetAssignmentsByAccountAsync(Guid accountId, bool activeOnly, CancellationToken cancellationToken)
        => ManagerPageDrain.FetchAllAsync<ManagerTransporterDeviceAssignmentVm>(
            (skip, take) => new GraphQLRequest
            {
                Query = TransporterDeviceAssignmentsByAccountQuery,
                Variables = new { accountId, activeOnly, skip, take }
            },
            (request, token) => QueryAsync<ManagerPage<ManagerTransporterDeviceAssignmentVm>>(request, token),
            "transporterDeviceAssignmentsByAccount",
            cancellationToken);
}
