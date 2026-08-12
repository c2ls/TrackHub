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

using Common.Domain.Enums;

namespace TrackHub.Router.Infrastructure.ManagerApi;

// Reads Manager account status via GraphQL. Backs the cached
// IAccountOperationalStatusService consumed by AccountStatusBehavior. Returns null for 0 (unknown).
public class AccountOperationalStatusReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IAccountOperationalStatusReader
{
    internal const string AccountStatusQuery = @"
                query($accountId: UUID!) {
                    accountStatus(query: { accountId: $accountId })
                }";

    public async Task<AccountStatus?> GetAccountStatusAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = AccountStatusQuery,
            Variables = new { accountId }
        };

        var status = await QueryAsync<short>(request, cancellationToken);
        return status == 0 ? null : (AccountStatus)status;
    }
}
