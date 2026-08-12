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
using TrackHub.Security.Application.Lookups;

namespace TrackHub.Security.Application.Users.Queries.GetLookup;

/// <summary>
/// Every member of the caller's account, id and username only. This is the left operand of the
/// allocator's set difference and must be complete.
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
        return LookupLimits.EnsureWithinCeiling(rows, "userLookupByAccount");
    }
}

/// <summary>
/// The members already holding a role — the right operand of the allocator's set difference.
/// </summary>
[Authorize(Resource = Resources.Users, Action = Actions.Read)]
public readonly record struct GetUserLookupByRoleQuery(int RoleId) : IRequest<IReadOnlyCollection<UserLookupVm>>;

public class GetUserLookupByRoleQueryHandler(IUserReader reader, IUser user)
    : IRequestHandler<GetUserLookupByRoleQuery, IReadOnlyCollection<UserLookupVm>>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<IReadOnlyCollection<UserLookupVm>> Handle(GetUserLookupByRoleQuery request, CancellationToken cancellationToken)
    {
        var caller = await reader.GetUserAsync(UserId, cancellationToken);
        var rows = await reader.GetUserLookupByRoleAsync(caller.AccountId, request.RoleId, LookupLimits.FetchSize, cancellationToken);
        return LookupLimits.EnsureWithinCeiling(rows, "userLookupByRole");
    }
}

/// <summary>
/// The members already holding a policy — the right operand of the allocator's set difference.
/// </summary>
[Authorize(Resource = Resources.Users, Action = Actions.Read)]
public readonly record struct GetUserLookupByPolicyQuery(int PolicyId) : IRequest<IReadOnlyCollection<UserLookupVm>>;

public class GetUserLookupByPolicyQueryHandler(IUserReader reader, IUser user)
    : IRequestHandler<GetUserLookupByPolicyQuery, IReadOnlyCollection<UserLookupVm>>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<IReadOnlyCollection<UserLookupVm>> Handle(GetUserLookupByPolicyQuery request, CancellationToken cancellationToken)
    {
        var caller = await reader.GetUserAsync(UserId, cancellationToken);
        var rows = await reader.GetUserLookupByPolicyAsync(caller.AccountId, request.PolicyId, LookupLimits.FetchSize, cancellationToken);
        return LookupLimits.EnsureWithinCeiling(rows, "userLookupByPolicy");
    }
}
