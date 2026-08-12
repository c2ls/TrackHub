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

using TrackHub.Geofencing.Application.TransportersInGeofence.Queries.Get;

namespace TrackHub.Geofencing.Web.GraphQL.Query;

public partial class Query
{

    // Filters stay as plain optional arguments (not an [AsParameters] input object) so the
    // existing no-argument documents from Reporting and the portal remain valid.
    public async Task<IReadOnlyCollection<TransporterInGeofenceVm>> GetTransportersInGeofence(
        [Service] ISender sender,
        Guid? geofenceId = null,
        short? type = null,
        CancellationToken cancellationToken = default)
        => await sender.Send(new GetTransportersInGeofenceQuery(geofenceId, type), cancellationToken);

}

