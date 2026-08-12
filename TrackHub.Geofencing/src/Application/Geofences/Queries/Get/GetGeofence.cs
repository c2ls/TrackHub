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

namespace TrackHub.Geofencing.Application.Geofences.Queries.Get;

[Authorize(Resource = Resources.Geofences, Action = Actions.Read)]
[AccountScopeEnforcedInHandler]
public readonly record struct GetGeofenceQuery(Guid Id) : IRequest<GeofenceVm>;

public class GetGeofenceQueryHandler(IGeofenceReader reader, IUserReader userReader, IUser user, IAccountFeatureReader accountFeatureReader) : IRequestHandler<GetGeofenceQuery, GeofenceVm>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<GeofenceVm> Handle(GetGeofenceQuery request, CancellationToken cancellationToken)
    {
        var result = await reader.GetGeofenceAsync(request.Id, cancellationToken);
        var currentUser = await userReader.GetUserAsync(UserId, cancellationToken);
        if (result.AccountId != currentUser.AccountId)
            throw new ForbiddenAccessException();
        await accountFeatureReader.EnsureFeatureEnabledAsync(currentUser.AccountId, FeatureKeys.Geofencing, cancellationToken);
        return result;
    }

}

