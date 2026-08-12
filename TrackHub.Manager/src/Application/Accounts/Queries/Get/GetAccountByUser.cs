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

namespace TrackHub.Manager.Application.Accounts.Queries.Get;

[Authorize(Resource = Resources.Accounts, Action = Actions.Read)]
public readonly record struct GetAccountByUserQuery() : IRequest<AccountVm>;

public class GetAccountByUserQueryHandler(IAccountReader reader, IUserReader usrReader, IUser user) : IRequestHandler<GetAccountByUserQuery, AccountVm>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();
    // This method handles the GetAccountQuery and returns an AccountVm
    public async Task<AccountVm> Handle(GetAccountByUserQuery request, CancellationToken cancellationToken)
    { 
        var user = await usrReader.GetUserAsync(UserId, cancellationToken);
        return await reader.GetAccountAsync(user.AccountId, cancellationToken); 
    }

}
