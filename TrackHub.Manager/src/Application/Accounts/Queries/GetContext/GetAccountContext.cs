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
using Common.Domain.Enums;

namespace TrackHub.Manager.Application.Accounts.Queries.GetContext;

// Single bootstrap read for the current principal's account (status + branding + features).
// [AllowSuspendedAccount] so portal/mobile can render a suspension state.
[Authorize(Resource = Resources.Accounts, Action = Actions.Read)]
[AllowSuspendedAccount]
public readonly record struct GetAccountContextQuery() : IRequest<AccountContextVm>;

public class GetAccountContextQueryHandler(
    IUser user,
    IUserReader userReader,
    IAccountOperationalStatusReader statusReader,
    IAccountBrandingReader brandingReader,
    IAccountFeatureReader featureReader) : IRequestHandler<GetAccountContextQuery, AccountContextVm>
{
    public async Task<AccountContextVm> Handle(GetAccountContextQuery request, CancellationToken cancellationToken)
    {
        var accountId = await ResolveAccountIdAsync(cancellationToken);

        var status = await statusReader.GetAccountStatusAsync(accountId, cancellationToken) ?? AccountStatus.Active;
        var branding = await brandingReader.GetBrandingAsync(accountId, cancellationToken);
        var features = await featureReader.GetAccountFeaturesAsync(accountId, cancellationToken);

        return new AccountContextVm(status, (short)status, branding, features);
    }

    private async Task<Guid> ResolveAccountIdAsync(CancellationToken cancellationToken)
    {
        if (user.AccountId.HasValue)
        {
            return user.AccountId.Value;
        }

        if (Guid.TryParse(user.Id, out var userId))
        {
            var current = await userReader.GetUserAsync(userId, cancellationToken);
            return current.AccountId;
        }

        throw new UnauthorizedAccessException();
    }
}
