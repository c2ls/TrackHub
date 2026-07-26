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
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

// This class represents a reader for retrieving group information from the database.
//
// Tenant scoping: GroupId is a sequential bigint and GetGroupQuery carries no top-level AccountId,
// so nothing upstream binds the key to the caller's tenant. See the note on UserReader.
public sealed class GroupReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IGroupReader
{
    // Retrieves a specific group by its ID.
    // Parameters:
    //   id: The ID of the group to retrieve.
    //   cancellationToken: A token to cancel the operation.
    // Returns:
    //   A Task that represents the asynchronous operation. The task result contains the GroupVm object.
    public async Task<GroupVm> GetGroupAsync(long id, CancellationToken cancellationToken)
    {
        var group = await Context.Groups
            .Where(d => d.GroupId.Equals(id))
            .Select(d => new GroupVm(
                d.GroupId,
                d.Name,
                d.Description,
                d.Active,
                d.AccountId))
            .FirstAsync(cancellationToken);

        RequireAccountAccess(group.AccountId);
        return group;
    }

    // Retrieves a page of the groups associated with a specific account.
    // Parameters:
    //   accountId: The ID of the account.
    //   skip / take: The page window.
    //   search: Optional name filter.
    //   cancellationToken: A token to cancel the operation.
    // Returns:
    //   A Task that represents the asynchronous operation. The task result contains a page of GroupVm objects.
    public async Task<GroupsPageVm> GetGroupsByAccountAsync(Guid accountId, int skip, int take, string? search, CancellationToken cancellationToken)
    {
        var query = ApplySearch(ByAccount(accountId), search);

        var totalCount = await query.CountAsync(cancellationToken);
        // Name alone is not unique within an account, so GroupId breaks the tie and gives Skip/Take a
        // total order; without it a page boundary repeats one group and hides another.
        var items = await query
            .OrderBy(g => g.Name)
            .ThenBy(g => g.GroupId)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new GroupsPageVm(items, totalCount);
    }

    // Minimal group projection for select controls: unpaged, capped by the caller.
    public async Task<IReadOnlyCollection<GroupLookupVm>> GetGroupLookupByAccountAsync(Guid accountId, int fetchSize, CancellationToken cancellationToken)
        => await Context.Groups
            .Where(g => g.AccountId == accountId)
            .OrderBy(g => g.Name)
            .ThenBy(g => g.GroupId)
            .Take(fetchSize)
            .Select(g => new GroupLookupVm(g.GroupId, g.Name))
            .ToListAsync(cancellationToken);

    // Filtered directly on Groups rather than joined through Accounts: the join was equivalent (a
    // group has exactly one account) but forced a Distinct to undo the fan-out it created.
    private IQueryable<GroupVm> ByAccount(Guid accountId)
        => Context.Groups
            .Where(g => g.AccountId == accountId)
            .Select(d => new GroupVm(
                d.GroupId,
                d.Name,
                d.Description,
                d.Active,
                d.AccountId));

    private static IQueryable<GroupVm> ApplySearch(IQueryable<GroupVm> query, string? search)
        => string.IsNullOrWhiteSpace(search)
            ? query
            : query.Where(g => EF.Functions.ILike(g.Name, SearchPattern.Contains(search), SearchPattern.Escape));

}
