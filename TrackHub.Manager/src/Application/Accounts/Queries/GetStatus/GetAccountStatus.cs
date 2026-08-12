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

namespace TrackHub.Manager.Application.Accounts.Queries.GetStatus;

// Lightweight cross-service status read. Mirrors validateFeatureEnabled: no
// [Authorize] gate (service clients read it during their own status enforcement), and
// [AllowSuspendedAccount] so a suspended account's own status is still readable — otherwise the
// cross-service status check would deadlock against itself. Returns 0 for an unknown account.
[AllowSuspendedAccount]
[AllowCrossAccount("Cross-service status probe. Router/SyncWorker and Reporting read it under their global service identity for the account they are about to act on — by definition not the caller's own. Returns a status code only, no tenant data.")]
public readonly record struct GetAccountStatusQuery(Guid AccountId) : IRequest<short>;

public class GetAccountStatusQueryHandler(IAccountOperationalStatusReader reader)
    : IRequestHandler<GetAccountStatusQuery, short>
{
    public async Task<short> Handle(GetAccountStatusQuery request, CancellationToken cancellationToken)
    {
        var status = await reader.GetAccountStatusAsync(request.AccountId, cancellationToken);
        return status.HasValue ? (short)status.Value : (short)0;
    }
}
