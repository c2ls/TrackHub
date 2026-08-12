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
using Common.Application.Paging;
using TrackHub.Manager.Application.Lookups;

namespace TrackHub.Manager.Application.Users.Queries.GetByGroup;

[Authorize(Resource = Resources.Users, Action = Actions.Read)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct GetUsersByGroupQuery(
    long GroupId,
    int? Skip,
    int? Take,
    string? Search) : IRequest<UsersPageVm>;

public class GetUsersQueryHandler(IUserReader reader) : IRequestHandler<GetUsersByGroupQuery, UsersPageVm>
{
    public async Task<UsersPageVm> Handle(GetUsersByGroupQuery request, CancellationToken cancellationToken)
    {
        var (skip, take) = PageRequest.Clamp(request.Skip, request.Take);
        return await reader.GetUsersByGroupAsync(request.GroupId, skip, take, request.Search, cancellationToken);
    }

}

/// <summary>
/// User picker feed for the caller's account: id and username only, unpaged, capped by
/// <see cref="LookupLimits.Ceiling"/>. The allocator dialogs subtract the already-assigned set from
/// this one, so a truncated list would offer members that are in fact already assigned.
/// </summary>
[Authorize(Resource = Resources.Users, Action = Actions.Read)]
public readonly record struct GetUserLookupByAccountQuery() : IRequest<IReadOnlyCollection<UserLookupVm>>;

public class GetUserLookupByAccountQueryHandler(IUserReader reader, IUser user)
    : IRequestHandler<GetUserLookupByAccountQuery, IReadOnlyCollection<UserLookupVm>>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<IReadOnlyCollection<UserLookupVm>> Handle(GetUserLookupByAccountQuery request, CancellationToken cancellationToken)
    {
        var caller = await reader.GetUserAsync(UserId, cancellationToken);
        var rows = await reader.GetUserLookupByAccountAsync(caller.AccountId, LookupLimits.FetchSize, cancellationToken);
        return LookupLimits.EnsureWithinCeiling(rows, "userLookup");
    }
}
