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

namespace TrackHub.Manager.Application.Accounts.Queries.GetBranding;

// Account-scoped branding read. [AllowSuspendedAccount] so the portal/mobile can render a branded
// suspension screen.
[Authorize(Resource = Resources.Accounts, Action = Actions.Read)]
[AllowSuspendedAccount]
public readonly record struct GetAccountBrandingQuery(Guid AccountId) : IRequest<AccountBrandingVm>;

public class GetAccountBrandingQueryHandler(IAccountBrandingReader reader)
    : IRequestHandler<GetAccountBrandingQuery, AccountBrandingVm>
{
    public async Task<AccountBrandingVm> Handle(GetAccountBrandingQuery request, CancellationToken cancellationToken)
        => await reader.GetBrandingAsync(request.AccountId, cancellationToken);
}

public sealed class GetAccountBrandingValidator : AbstractValidator<GetAccountBrandingQuery>
{
    public GetAccountBrandingValidator()
        => RuleFor(x => x.AccountId).NotEmpty();
}
