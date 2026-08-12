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

using TrackHub.Reporting.Domain.Interfaces.Telemetry;
using TrackHub.Reporting.Domain.Models.Manager;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

public class GpsTelemetryReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Telemetry)), IGpsTelemetryReader
{
    // Single source of truth for the queries this reader sends; the
    // ServiceContracts tests validate these exact strings against the Telemetry schema.
    internal const string OperatorHealthSummaryQuery = @"
                query($operatorId: UUID!, $lookbackHours: Int!) {
                    operatorHealthSummary(query: { operatorId: $operatorId, lookbackHours: $lookbackHours }) {
                        operatorId
                        since
                        totalChecks
                        healthyChecks
                        degradedChecks
                        offlineChecks
                        failureCount
                        uptimePercent
                        averageLatencyMs
                        lastCheckAt
                        lastFailureAt
                        lastFailureCode
                    }
                }";

    internal const string OperatorSyncRunsQuery = @"
                query($accountId: UUID, $operatorId: UUID, $take: Int!) {
                    operatorSyncRuns(query: { accountId: $accountId, operatorId: $operatorId, take: $take }) {
                        operatorSyncRunId
                        accountId
                        operatorId
                        triggerType
                        result
                        startedAt
                        completedAt
                        devicesSeen
                        devicesAdded
                        devicesUpdated
                        devicesRemoved
                        devicesIgnored
                        positionsRead
                        positionsAccepted
                        positionsRejected
                        errorCode
                        errorMessage
                        correlationId
                    }
                }";

    internal const string TransporterPositionByOperatorQuery = @"
                query($operatorId: UUID!) {
                    transporterPositionByOperator(query: { operatorId: $operatorId }) {
                        transporterId
                        deviceName
                        deviceDateTime
                    }
                }";

    internal const string PositionHistoryQuery = @"
                query($accountId: UUID!, $transporterId: UUID, $deviceId: UUID, $take: Int!) {
                    positionHistory(query: { accountId: $accountId, transporterId: $transporterId, deviceId: $deviceId, take: $take }) {
                        transporterPositionHistoryId
                        accountId
                        operatorId
                        deviceId
                        transporterId
                        sourceTimestamp
                        receivedAt
                        latitude
                        longitude
                    }
                }";

    public async Task<ManagerOperatorHealthSummaryVm> GetOperatorHealthSummaryAsync(Guid operatorId, int lookbackHours, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = OperatorHealthSummaryQuery,
            Variables = new { operatorId, lookbackHours }
        };
        return await QueryAsync<ManagerOperatorHealthSummaryVm>(request, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ManagerOperatorSyncRunVm>> GetOperatorSyncRunsAsync(Guid accountId, Guid? operatorId, int take, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = OperatorSyncRunsQuery,
            Variables = new
            {
                accountId = (Guid?)accountId,
                operatorId,
                take
            }
        };
        return await QueryAsync<List<ManagerOperatorSyncRunVm>>(request, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ManagerTransporterPositionVm>> GetLatestPositionsAsync(Guid operatorId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = TransporterPositionByOperatorQuery,
            Variables = new { operatorId }
        };
        return await QueryAsync<List<ManagerTransporterPositionVm>>(request, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ManagerTransporterPositionHistoryVm>> GetPositionHistoryAsync(Guid accountId, Guid? transporterId, Guid? deviceId, int take, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = PositionHistoryQuery,
            Variables = new
            {
                accountId,
                transporterId,
                deviceId,
                take
            }
        };
        return await QueryAsync<List<ManagerTransporterPositionHistoryVm>>(request, cancellationToken);
    }
}
