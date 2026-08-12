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

public sealed class PointOfInterestReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IPointOfInterestReader
{
    public async Task<PointOfInterestVm> GetPointOfInterestAsync(Guid id, CancellationToken cancellationToken)
    {
        var poi = await Context.PointsOfInterest
            .Where(p => p.PointOfInterestId == id)
            .Select(p => new PointOfInterestVm(
                p.PointOfInterestId,
                p.AccountId,
                p.Name,
                p.Description,
                p.Type,
                p.Latitude,
                p.Longitude,
                p.Address,
                p.Color,
                p.GroupId,
                p.Active))
            .FirstAsync(cancellationToken);

        RequireAccountAccess(poi.AccountId);
        return poi;
    }

    // When visibleToUserId is set, POIs restricted to a Group outside that user's groups
    // are excluded; POIs with no Group are visible to every account user with read access.
    public async Task<PointsOfInterestPageVm> GetPointsOfInterestByAccountAsync(Guid accountId, Guid? visibleToUserId, int skip, int take, string? search, CancellationToken cancellationToken)
    {
        var query = ApplySearch(Visible(accountId, visibleToUserId), search);

        var totalCount = await query.CountAsync(cancellationToken);
        // POI names repeat freely (every depot may hold several "Gate"), so the primary key is what
        // makes the ordering total and the page window stable.
        var items = await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.PointOfInterestId)
            .Skip(skip)
            .Take(take)
            .Select(p => new PointOfInterestVm(
                p.PointOfInterestId,
                p.AccountId,
                p.Name,
                p.Description,
                p.Type,
                p.Latitude,
                p.Longitude,
                p.Address,
                p.Color,
                p.GroupId,
                p.Active))
            .ToListAsync(cancellationToken);

        return new PointsOfInterestPageVm(items, totalCount);
    }

    // Picker projection. Carries what the map overlays draw — coordinates, pin colour and the
    // popup's type/description/address — so the dashboard and route planner render from this feed
    // instead of draining the paged read. Honours the same group-visibility narrowing as the paged
    // read; AccountId and GroupId stay off it because nothing renders them.
    public async Task<IReadOnlyCollection<PointOfInterestLookupVm>> GetPointOfInterestLookupAsync(Guid accountId, Guid? visibleToUserId, int fetchSize, CancellationToken cancellationToken)
        => await Visible(accountId, visibleToUserId)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.PointOfInterestId)
            .Take(fetchSize)
            .Select(p => new PointOfInterestLookupVm(
                p.PointOfInterestId,
                p.Name,
                p.Latitude,
                p.Longitude,
                p.Color,
                p.Type,
                p.Description,
                p.Address,
                p.Active))
            .ToListAsync(cancellationToken);

    private IQueryable<Entities.PointOfInterest> Visible(Guid accountId, Guid? visibleToUserId)
    {
        var scopedAccountId = RequireAccountAccess(accountId);

        var query = Context.PointsOfInterest.Where(p => p.AccountId == scopedAccountId);

        if (visibleToUserId.HasValue)
        {
            var userId = visibleToUserId.Value;
            var userGroupIds = Context.Users
                .Where(u => u.UserId == userId)
                .SelectMany(u => u.Groups.Select(g => g.GroupId));

            query = query.Where(p => p.GroupId == null || userGroupIds.Contains(p.GroupId.Value));
        }

        return query;
    }

    private static IQueryable<Entities.PointOfInterest> ApplySearch(IQueryable<Entities.PointOfInterest> query, string? search)
        => string.IsNullOrWhiteSpace(search)
            ? query
            : query.Where(p => EF.Functions.ILike(p.Name, SearchPattern.Contains(search), SearchPattern.Escape));
}
