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

using Common.Application.Paging;

namespace TrackHub.Manager.Application.Accounts.Queries.GetAll;

[Authorize(Resource = Resources.Administrative, Action = Actions.Read)]
public readonly record struct GetAccountsQuery(
    int? Skip,
    int? Take,
    string? Search) : IRequest<AccountsPageVm>;

public class GetAccountsQueryHandler(IAccountReader reader) : IRequestHandler<GetAccountsQuery, AccountsPageVm>
{
    // This method handles the GetAccountsQuery by retrieving a page of accounts from the reader asynchronously
    public async Task<AccountsPageVm> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var (skip, take) = PageRequest.Clamp(request.Skip, request.Take);
        return await reader.GetAccountsAsync(skip, take, request.Search, cancellationToken);
    }

}
