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
using TransporterType = Common.Domain.Enums.TransporterType;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

// TransporterReader class for reading transporter information
// Tenant scoping: TransporterVm deliberately does not expose AccountId, so the owning account is
// read alongside the projection and authorized before the row is returned. See the note on
// UserReader for why this cannot be left to AccountScopeBehavior.
public sealed class TransporterReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), ITransporterReader
{
    /// <summary>
    /// GetTransporterAsync method retrieves a transporter by its ID
    /// </summary>
    /// <param name="id">The ID of the transporter</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the TransporterVm object.</returns>
    public async Task<TransporterVm> GetTransporterAsync(Guid id, CancellationToken cancellationToken)
    {
        var row = await Context.Transporters
            .Where(d => d.TransporterId.Equals(id))
            .Select(d => new
            {
                d.AccountId,
                Vm = new TransporterVm(
                    d.TransporterId,
                    d.Name,
                    (TransporterType)d.TransporterTypeId,
                    d.TransporterTypeId)
            })
            .FirstAsync(cancellationToken);

        RequireAccountAccess(row.AccountId);
        return row.Vm;
    }

    /// <summary>
    /// GetTransporterAsync method retrieves a transporter by its name
    /// </summary>
    /// <param name="name">The name of the transporter</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the TransporterVm object.</returns>
    public async Task<TransporterVm> GetTransporterAsync(string name, CancellationToken cancellationToken)
        => await Context.Transporters
            .Where(d => d.Name.Equals(name))
            .Select(d => new TransporterVm(
                d.TransporterId,
                d.Name,
                (TransporterType)d.TransporterTypeId,
                d.TransporterTypeId))
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// GetTransportersByGroupAsync method retrieves a page of transporters by group ID
    /// </summary>
    /// <param name="groupId">The ID of the group</param>
    /// <param name="skip">Rows to skip</param>
    /// <param name="take">Rows to return</param>
    /// <param name="search">Optional name filter</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a page of TransporterVm objects.</returns>
    public async Task<TransportersPageVm> GetTransportersByGroupAsync(long groupId, int skip, int take, string? search, CancellationToken cancellationToken)
    {
        // Sequential bigint key — see UserReader.GetUsersByGroupAsync.
        var groupAccountId = await Context.Groups
            .Where(g => g.GroupId == groupId)
            .Select(g => (Guid?)g.AccountId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!groupAccountId.HasValue)
        {
            return new TransportersPageVm([], 0);
        }

        RequireAccountAccess(groupAccountId.Value);

        var query = ApplySearch(
            Context.Groups
                .Where(g => g.GroupId == groupId)
                .SelectMany(g => g.Transporters)
                .Distinct(),
            search);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(d => d.Name)
            .ThenBy(d => d.TransporterId)
            .Skip(skip)
            .Take(take)
            .Select(d => new TransporterVm(
                d.TransporterId,
                d.Name,
                (TransporterType)d.TransporterTypeId,
                d.TransporterTypeId))
            .ToListAsync(cancellationToken);

        return new TransportersPageVm(items, totalCount);
    }

    /// <summary>
    /// GetTransportersByUserAsync method retrieves a page of transporters visible to a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="skip">Rows to skip</param>
    /// <param name="take">Rows to return</param>
    /// <param name="search">Optional name filter</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a page of TransporterVm objects.</returns>
    public async Task<TransportersPageVm> GetTransportersByUserAsync(Guid userId, int skip, int take, string? search, CancellationToken cancellationToken)
    {
        var subjectAccountId = await Context.Users
            .Where(u => u.UserId == userId)
            .Select(u => (Guid?)u.AccountId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!subjectAccountId.HasValue)
        {
            return new TransportersPageVm([], 0);
        }

        RequireAccountAccess(subjectAccountId.Value);

        var query = ApplySearch(VisibleToUser(userId), search);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(d => d.Name)
            .ThenBy(d => d.TransporterId)
            .Skip(skip)
            .Take(take)
            .Select(d => new TransporterVm(
                d.TransporterId,
                d.Name,
                (TransporterType)d.TransporterTypeId,
                d.TransporterTypeId))
            .ToListAsync(cancellationToken);

        return new TransportersPageVm(items, totalCount);
    }

    /// <summary>
    /// GetTransportersByAccountAsync method retrieves a page of transporters by account ID
    /// </summary>
    /// <param name="accountId">The ID of the account</param>
    /// <param name="skip">Rows to skip</param>
    /// <param name="take">Rows to return</param>
    /// <param name="search">Optional name filter</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a page of TransporterVm objects.</returns>
    public async Task<TransportersPageVm> GetTransportersByAccountAsync(Guid accountId, int skip, int take, string? search, CancellationToken cancellationToken)
    {
        var query = ApplySearch(ByAccount(accountId), search);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(t => t.Name)
            .ThenBy(t => t.TransporterId)
            .Skip(skip)
            .Take(take)
            .Select(t => new TransporterVm(
                t.TransporterId,
                t.Name,
                (TransporterType)t.TransporterTypeId,
                t.TransporterTypeId))
            .ToListAsync(cancellationToken);

        return new TransportersPageVm(items, totalCount);
    }

    /// <summary>
    /// Minimal account-wide transporter projection for select controls. Returns up to
    /// <paramref name="fetchSize"/> rows so the caller can detect an over-ceiling set and raise
    /// rather than bind a truncated picker.
    /// </summary>
    public async Task<IReadOnlyCollection<TransporterLookupVm>> GetTransporterLookupByAccountAsync(Guid accountId, int fetchSize, CancellationToken cancellationToken)
        => await ByAccount(accountId)
            .OrderBy(t => t.Name)
            .ThenBy(t => t.TransporterId)
            .Take(fetchSize)
            .Select(t => new TransporterLookupVm(
                t.TransporterId,
                t.Name,
                (TransporterType)t.TransporterTypeId,
                t.TransporterTypeId))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Minimal transporter projection for select controls, narrowed to the transporters the user's
    /// groups make visible. Same ceiling semantics as the account-wide lookup.
    /// </summary>
    public async Task<IReadOnlyCollection<TransporterLookupVm>> GetTransporterLookupByUserAsync(Guid userId, int fetchSize, CancellationToken cancellationToken)
    {
        var subjectAccountId = await Context.Users
            .Where(u => u.UserId == userId)
            .Select(u => (Guid?)u.AccountId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!subjectAccountId.HasValue)
        {
            return [];
        }

        RequireAccountAccess(subjectAccountId.Value);

        return await VisibleToUser(userId)
            .OrderBy(t => t.Name)
            .ThenBy(t => t.TransporterId)
            .Take(fetchSize)
            .Select(t => new TransporterLookupVm(
                t.TransporterId,
                t.Name,
                (TransporterType)t.TransporterTypeId,
                t.TransporterTypeId))
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Entities.Transporter> ByAccount(Guid accountId)
        => Context.Transporters
            .Where(t => t.AccountId == accountId);

    private IQueryable<Entities.Transporter> VisibleToUser(Guid userId)
        => Context.Users
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.Groups)
            .SelectMany(g => g.Transporters)
            .Distinct();

    private static IQueryable<Entities.Transporter> ApplySearch(IQueryable<Entities.Transporter> query, string? search)
        => string.IsNullOrWhiteSpace(search)
            ? query
            : query.Where(t => EF.Functions.ILike(t.Name, SearchPattern.Contains(search), SearchPattern.Escape));

    /// <summary>
    /// Returns the owning <c>AccountId</c> for the given transporter, or <c>null</c> when not found.
    /// Used by handlers that need account scope without leaking the EF context to Application.
    /// </summary>
    public async Task<Guid?> GetAccountIdAsync(Guid transporterId, CancellationToken cancellationToken)
        => await Context.Transporters
            .Where(t => t.TransporterId == transporterId)
            .Select(t => (Guid?)t.AccountId)
            .FirstOrDefaultAsync(cancellationToken);
}
