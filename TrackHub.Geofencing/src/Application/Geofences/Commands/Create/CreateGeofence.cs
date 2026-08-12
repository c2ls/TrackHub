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

namespace TrackHub.Geofencing.Application.Geofences.Commands.Create;

[Authorize(Resource = Resources.Geofences, Action = Actions.Write)]
// Caller-bound create: the handler resolves the caller's own account and the writer binds the new
// row to it (GeofenceWriter.CreateGeofenceAsync ignores any other tenant signal). The DTO's
// GeofenceId is the client-generated identity of the NEW row, not a reference — the INSERT cannot
// reach an existing geofence (a colliding id fails on the primary key).
[AccountScopeEnforcedInHandler]
public readonly record struct CreateGeofenceCommand(GeofenceDto Geofence) : IRequest<GeofenceVm>;

public class CreateGeofenceCommandHandler(IGeofenceWriter writer, IUserReader userReader, IUser user, IAccountFeatureReader accountFeatureReader) : IRequestHandler<CreateGeofenceCommand, GeofenceVm>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();
    public async Task<GeofenceVm> Handle(CreateGeofenceCommand request, CancellationToken cancellationToken)
    {
        var user = await userReader.GetUserAsync(UserId, cancellationToken);
        await accountFeatureReader.EnsureFeatureEnabledAsync(user.AccountId, FeatureKeys.Geofencing, cancellationToken);
        return await writer.CreateGeofenceAsync(request.Geofence, user.AccountId, cancellationToken); 
    }
}

