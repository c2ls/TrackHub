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

// This class represents a reader for retrieving user data from the database.
//
// Tenant scoping: the by-id and by-group reads take a caller-supplied key, and their queries carry no
// top-level AccountId, so AccountScopeBehavior has no account to compare and this DbContext has no
// global query filter. Scoping happens here, where the row's owning account first becomes known.
// RequireAccountAccess admits a global service client and an active support grant, so internal and
// support flows are unaffected.
public sealed class UserReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IUserReader
{
    /// <summary>
    /// Retrieves a user by their ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A UserVm object representing the retrieved user.</returns>
    public async Task<UserVm> GetUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await Context.Users
            .Where(u => u.UserId.Equals(id))
            .Select(u => new UserVm(
                u.UserId,
                u.Username,
                u.Active,
                u.AccountId))
            .FirstAsync(cancellationToken);

        RequireAccountAccess(user.AccountId);
        return user;
    }

    /// <summary>
    /// Retrieves a page of the users in a group.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="search"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A UsersPageVm carrying the requested window and the unpaged total.
    /// </returns>
    public async Task<UsersPageVm> GetUsersByGroupAsync(long groupId, int skip, int take, string? search, CancellationToken cancellationToken)
    {
        // GroupId is a sequential bigint and therefore directly enumerable, so the group's owning
        // account is resolved first and authorized before any user row is projected.
        var groupAccountId = await Context.Groups
            .Where(g => g.GroupId == groupId)
            .Select(g => (Guid?)g.AccountId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!groupAccountId.HasValue)
        {
            return new UsersPageVm([], 0);
        }

        RequireAccountAccess(groupAccountId.Value);

        var query = ApplySearch(
            Context.Groups
                .Where(g => g.GroupId == groupId)
                .SelectMany(g => g.Users)
                .Select(u => new UserVm(
                    u.UserId,
                    u.Username,
                    u.Active,
                    u.AccountId))
                .Distinct(),
            search);

        var totalCount = await query.CountAsync(cancellationToken);
        // UserId is the tiebreaker: usernames are unique per account today, but the page window must
        // not depend on that invariant holding across accounts sharing a group.
        var items = await query
            .OrderBy(u => u.Username)
            .ThenBy(u => u.UserId)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new UsersPageVm(items, totalCount);
    }

    /// <summary>
    /// Minimal user projection for select controls: unpaged, capped by the caller.
    /// </summary>
    public async Task<IReadOnlyCollection<UserLookupVm>> GetUserLookupByAccountAsync(Guid accountId, int fetchSize, CancellationToken cancellationToken)
    {
        var scoped = RequireAccountAccess(accountId);
        return await Context.Users
            .Where(u => u.AccountId == scoped)
            .OrderBy(u => u.Username)
            .ThenBy(u => u.UserId)
            .Take(fetchSize)
            .Select(u => new UserLookupVm(u.UserId, u.Username))
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<UserVm> ApplySearch(IQueryable<UserVm> query, string? search)
        => string.IsNullOrWhiteSpace(search)
            ? query
            : query.Where(u => EF.Functions.ILike(u.Username, SearchPattern.Contains(search), SearchPattern.Escape));
}
