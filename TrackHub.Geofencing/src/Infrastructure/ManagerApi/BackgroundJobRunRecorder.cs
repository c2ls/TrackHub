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

namespace TrackHub.Geofencing.Infrastructure.ManagerApi;

/// <summary>
/// Records background job runs via Manager's <c>createBackgroundJobRun</c> under the
/// <c>geofence_client</c> identity (Geofencing has no local BackgroundJobRun table).
/// </summary>
public class BackgroundJobRunRecorder(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager, asService: true)), IBackgroundJobRunRecorder
{
    internal const string CreateBackgroundJobRunMutation = @"
                mutation($command: CreateBackgroundJobRunCommandInput!) {
                    createBackgroundJobRun(command: $command) { backgroundJobRunId }
                }";

    public async Task RecordAsync(
        string jobKey,
        string? resourceKey,
        string idempotencyKey,
        string status,
        DateTimeOffset startedAt,
        DateTimeOffset? completedAt,
        CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = CreateBackgroundJobRunMutation,
            Variables = new
            {
                command = new
                {
                    backgroundJobRun = new
                    {
                        jobKey,
                        accountId = (Guid?)null,
                        resourceKey,
                        idempotencyKey,
                        status,
                        attempts = 1,
                        startedAt,
                        completedAt,
                        errorCode = (string?)null,
                        errorMessage = (string?)null
                    }
                }
            }
        };
        await MutationAsync<object>(request, cancellationToken);
    }
}
