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

namespace TrackHub.Geofencing.Application.Geofences.Queries.GetByAccount;

// No [Caching]: the cache key is built from request properties only and this query resolves
// the account from the caller, so a cached page would be served across
// accounts (never put [Caching] on per-user/per-account queries). EnableCaching stays in the
// contract for compatibility but is inert.
[Authorize(Resource = Resources.Geofences, Action = Actions.Read)]
public readonly record struct GetGeofencesByAccountQuery(
    bool EnableCaching,
    int? Skip,
    int? Take,
    short? Type,
    bool? Active,
    string? Search) : IRequest<GeofencesPageVm>;

public class GetGeofencesByAccountQueryHandler(IGeofenceReader reader, IUserReader userReader, IUser user, IAccountFeatureReader accountFeatureReader) : IRequestHandler<GetGeofencesByAccountQuery, GeofencesPageVm>
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 500;

    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();
    public async Task<GeofencesPageVm> Handle(GetGeofencesByAccountQuery request, CancellationToken cancellationToken)
    {
        var user = await userReader.GetUserAsync(UserId, cancellationToken);
        await accountFeatureReader.EnsureFeatureEnabledAsync(user.AccountId, FeatureKeys.Geofencing, cancellationToken);

        var skip = Math.Max(request.Skip ?? 0, 0);
        var take = Math.Clamp(request.Take ?? DefaultPageSize, 1, MaxPageSize);
        return await reader.GetGeofencesPageAsync(
            user.AccountId, skip, take, request.Type, request.Active, request.Search, cancellationToken);
    }

}
