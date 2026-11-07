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

namespace TrackHub.Security.Infrastructure.ManagerApi;

public class ManagerStaffMemberReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager, asService: true)), IManagerStaffMemberReader
{
    // The ServiceContracts tests validate this document against Manager's exported schema.
    internal const string ValidateStaffAccountQuery = @"
                query($staffId: UUID!, $accountId: UUID!) {
                    validateStaffAccount(query: { staffId: $staffId, accountId: $accountId })
                }";

    public async Task<bool> StaffBelongsToAccountAsync(Guid staffId, Guid accountId, CancellationToken cancellationToken)
        => await QueryAsync<bool>(new GraphQLRequest
        {
            Query = ValidateStaffAccountQuery,
            Variables = new { staffId, accountId }
        }, cancellationToken);
}
