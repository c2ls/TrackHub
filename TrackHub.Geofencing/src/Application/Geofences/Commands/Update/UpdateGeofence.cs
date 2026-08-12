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

namespace TrackHub.Geofencing.Application.Geofences.Commands.Update;

[Authorize(Resource = Resources.Geofences, Action = Actions.Edit)]
[AccountScopeEnforcedInHandler]
public readonly record struct UpdateGeofenceCommand(GeofenceDto Geofence) : IRequest;

public class UpdateGeofenceCommandHandler(IGeofenceWriter writer, IGeofenceReader reader, IUserReader userReader, IUser user, IAccountFeatureReader accountFeatureReader) : IRequestHandler<UpdateGeofenceCommand>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task Handle(UpdateGeofenceCommand request, CancellationToken cancellationToken)
    {
        var currentGeofence = await reader.GetGeofenceAsync(request.Geofence.GeofenceId, cancellationToken);
        var currentUser = await userReader.GetUserAsync(UserId, cancellationToken);
        if (currentGeofence.AccountId != currentUser.AccountId)
            throw new ForbiddenAccessException();

        await accountFeatureReader.EnsureFeatureEnabledAsync(currentUser.AccountId, FeatureKeys.Geofencing, cancellationToken);
        await writer.UpdateGeofenceAsync(request.Geofence, cancellationToken);
    }
}

