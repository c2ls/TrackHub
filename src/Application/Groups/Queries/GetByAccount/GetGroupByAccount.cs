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

namespace TrackHub.Manager.Application.Groups.Queries.GetByAccount;

[Authorize(Resource = Resources.Groups, Action = Actions.Read)]
public readonly record struct GetGroupByAccountQuery(
    int? Skip,
    int? Take,
    string? Search) : IRequest<GroupsPageVm>;

public class GetGroupsQueryHandler(IGroupReader reader, IUserReader userReader, IUser user) : IRequestHandler<GetGroupByAccountQuery, GroupsPageVm>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<GroupsPageVm> Handle(GetGroupByAccountQuery request, CancellationToken cancellationToken)
    {
        var user = await userReader.GetUserAsync(UserId, cancellationToken);
        var (skip, take) = PageRequest.Clamp(request.Skip, request.Take);
        return await reader.GetGroupsByAccountAsync(user.AccountId, skip, take, request.Search, cancellationToken);
    }

}

/// <summary>
/// Group picker feed for the caller's account: id and name only, unpaged, capped by
/// <see cref="LookupLimits.Ceiling"/>.
/// </summary>
[Authorize(Resource = Resources.Groups, Action = Actions.Read)]
public readonly record struct GetGroupLookupByAccountQuery() : IRequest<IReadOnlyCollection<GroupLookupVm>>;

public class GetGroupLookupByAccountQueryHandler(IGroupReader reader, IUserReader userReader, IUser user)
    : IRequestHandler<GetGroupLookupByAccountQuery, IReadOnlyCollection<GroupLookupVm>>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<IReadOnlyCollection<GroupLookupVm>> Handle(GetGroupLookupByAccountQuery request, CancellationToken cancellationToken)
    {
        var user = await userReader.GetUserAsync(UserId, cancellationToken);
        var rows = await reader.GetGroupLookupByAccountAsync(user.AccountId, LookupLimits.FetchSize, cancellationToken);
        return LookupLimits.EnsureWithinCeiling(rows, "groupLookup");
    }
}
