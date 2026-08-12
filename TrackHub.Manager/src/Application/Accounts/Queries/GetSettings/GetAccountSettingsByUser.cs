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

namespace TrackHub.Manager.Application.Accounts.Queries.GetSettings;

[Authorize(Resource = Resources.Accounts, Action = Actions.Read)]
public readonly record struct GetAccountSettingsByUserQuery() : IRequest<AccountSettingsVm>;

public class GetAccountSettingsByUserQueryHandler(IAccountSettingsReader reader, IUserReader userReader, IUser user) : IRequestHandler<GetAccountSettingsByUserQuery, AccountSettingsVm>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();
    // This method handles the GetAccountSettingsQuery and returns an AccountSettingsVm
    public async Task<AccountSettingsVm> Handle(GetAccountSettingsByUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userReader.GetUserAsync(UserId, cancellationToken);
        return await reader.GetAccountSettingsAsync(user.AccountId, cancellationToken);
    }

}
