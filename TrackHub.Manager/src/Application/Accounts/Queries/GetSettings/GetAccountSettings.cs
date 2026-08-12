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

namespace TrackHub.Manager.Application.Accounts.Queries.GetSettings;

[Authorize(Resource = Resources.Accounts, Action = Actions.Read)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct GetAccountSettingsQuery(Guid Id) : IRequest<AccountSettingsVm>;

public class GetAccountSettingsQueryHandler(IAccountSettingsReader reader) : IRequestHandler<GetAccountSettingsQuery, AccountSettingsVm>
{
    public async Task<AccountSettingsVm> Handle(GetAccountSettingsQuery request, CancellationToken cancellationToken)
        => await reader.GetAccountSettingsAsync(request.Id, cancellationToken);
}
