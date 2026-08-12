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

namespace TrackHub.Geofencing.Application.TransportersInGeofence.Queries.Get;

[Authorize(Resource = Resources.Geofencing, Action = Actions.Read)]
public readonly record struct GetTransportersInGeofenceQuery(
    Guid? GeofenceId,
    short? Type) : IRequest<IReadOnlyCollection<TransporterInGeofenceVm>>;

public class GetTransportersInGeofenceQueryHandler(ITransportersInGeofence reader, IUserReader userReader, IUser user, IAccountFeatureReader accountFeatureReader) : IRequestHandler<GetTransportersInGeofenceQuery, IReadOnlyCollection<TransporterInGeofenceVm>>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<IReadOnlyCollection<TransporterInGeofenceVm>> Handle(GetTransportersInGeofenceQuery request, CancellationToken cancellationToken)
    {
        var user = await userReader.GetUserAsync(UserId, cancellationToken);
        await accountFeatureReader.EnsureFeatureEnabledAsync(user.AccountId, FeatureKeys.Geofencing, cancellationToken);
        return await reader.GetTransportersInGeofencesAsync(user.AccountId, UserId, request.GeofenceId, request.Type, cancellationToken);
    }

}
