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

namespace TrackHub.Manager.Application.PointsOfInterest.Queries.GetByAccount;

[Authorize(Resource = Resources.PointsOfInterest, Action = Actions.Read)]
public readonly record struct GetPointsOfInterestByAccountQuery(
    int? Skip,
    int? Take,
    string? Search) : IRequest<PointsOfInterestPageVm>;

public class GetPointsOfInterestByAccountQueryHandler(
    IPointOfInterestReader reader,
    IUserReader userReader,
    ICurrentPrincipal principal,
    IUser user) : IRequestHandler<GetPointsOfInterestByAccountQuery, PointsOfInterestPageVm>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<PointsOfInterestPageVm> Handle(GetPointsOfInterestByAccountQuery request, CancellationToken cancellationToken)
    {
        var userVm = await userReader.GetUserAsync(UserId, cancellationToken);
        var (skip, take) = PageRequest.Clamp(request.Skip, request.Take);

        return await reader.GetPointsOfInterestByAccountAsync(
            userVm.AccountId, VisibilityScope(principal, UserId), skip, take, request.Search, cancellationToken);
    }

    // Administrators and managers see every POI in the account; other users only see
    // POIs without a group restriction or restricted to one of their own groups.
    internal static Guid? VisibilityScope(ICurrentPrincipal principal, Guid userId)
        => string.Equals(principal.Role, Roles.Administrator, StringComparison.OrdinalIgnoreCase)
            || string.Equals(principal.Role, Roles.Manager, StringComparison.OrdinalIgnoreCase)
            ? null
            : userId;
}

/// <summary>
/// Point-of-interest picker feed. Carries coordinates as well as id and name because the dashboard
/// plots the picked points on the map straight from this projection. Unpaged and capped by
/// <see cref="LookupLimits.Ceiling"/>.
/// </summary>
[Authorize(Resource = Resources.PointsOfInterest, Action = Actions.Read)]
public readonly record struct GetPointOfInterestLookupQuery() : IRequest<IReadOnlyCollection<PointOfInterestLookupVm>>;

public class GetPointOfInterestLookupQueryHandler(
    IPointOfInterestReader reader,
    IUserReader userReader,
    ICurrentPrincipal principal,
    IUser user) : IRequestHandler<GetPointOfInterestLookupQuery, IReadOnlyCollection<PointOfInterestLookupVm>>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<IReadOnlyCollection<PointOfInterestLookupVm>> Handle(GetPointOfInterestLookupQuery request, CancellationToken cancellationToken)
    {
        var userVm = await userReader.GetUserAsync(UserId, cancellationToken);
        var rows = await reader.GetPointOfInterestLookupAsync(
            userVm.AccountId,
            GetPointsOfInterestByAccountQueryHandler.VisibilityScope(principal, UserId),
            LookupLimits.FetchSize,
            cancellationToken);

        return LookupLimits.EnsureWithinCeiling(rows, "pointOfInterestLookup");
    }
}
