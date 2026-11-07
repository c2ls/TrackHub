// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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
using TrackHub.Manager.Domain.Interfaces;

namespace TrackHub.Manager.Infrastructure.SecurityApi;

// Runs under Manager's manager_client service identity: the operator deactivating a staff holds
// Manager's Staff grants, not necessarily Security's, and the credential must end regardless.
public class StaffCredentialRevoker(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Security, asService: true)), IStaffCredentialRevoker
{
    internal const string RevokeStaffCredentialsMutation = @"
                mutation($staffId: UUID!, $accountId: UUID!) {
                    revokeStaffCredentials(command: { staffId: $staffId, accountId: $accountId })
                }";

    public async Task RevokeStaffCredentialsAsync(Guid staffId, Guid accountId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = RevokeStaffCredentialsMutation,
            Variables = new { staffId, accountId }
        };
        await MutationAsync<bool>(request, cancellationToken);
    }
}
