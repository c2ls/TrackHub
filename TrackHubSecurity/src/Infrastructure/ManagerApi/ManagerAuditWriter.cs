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

using Common.Application.Interfaces;
using Common.Domain.Constants;
using Common.Infrastructure;
using GraphQL;
using TrackHub.Security.Domain.Interfaces;
using TrackHub.Security.Domain.Models;
using TrackHub.Security.Domain.Records;

namespace TrackHub.Security.Infrastructure.ManagerApi;

// Sends security audit events to Manager's CreateAuditEvent resolver using the security_client
// service identity (asService). The ServiceContracts tests validate this document against Manager's schema.
public class ManagerAuditWriter(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager, asService: true)), IManagerAuditWriter
{
    internal const string CreateAuditEventMutation = @"
                    mutation($accountId: UUID!, $actorType: String!, $actorId: String!, $action: String!, $resourceType: String!, $resourceId: String!, $result: String!, $oldValuesJson: String, $newValuesJson: String, $correlationId: String) {
                      createAuditEvent(command: { auditEvent: { accountId: $accountId, actorType: $actorType, actorId: $actorId, action: $action, resourceType: $resourceType, resourceId: $resourceId, result: $result, oldValuesJson: $oldValuesJson, newValuesJson: $newValuesJson, correlationId: $correlationId } }) {
                        auditEventId
                      }
                    }";

    public async Task ForwardAuditEventAsync(SecurityAuditEventDto auditEvent, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = CreateAuditEventMutation,
            Variables = new
            {
                accountId = auditEvent.AccountId ?? Guid.Empty,
                actorType = auditEvent.ActorType,
                actorId = auditEvent.ActorId,
                action = auditEvent.Action,
                resourceType = auditEvent.ResourceType,
                resourceId = auditEvent.ResourceId,
                result = "Succeeded",
                oldValuesJson = auditEvent.OldValuesJson,
                newValuesJson = auditEvent.NewValuesJson,
                correlationId = auditEvent.CorrelationId
            }
        };

        await MutationAsync<AuditEventForwardVm>(request, cancellationToken);
    }
}
