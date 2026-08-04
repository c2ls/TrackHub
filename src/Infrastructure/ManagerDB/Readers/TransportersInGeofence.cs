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

namespace TrackHub.Geofencing.Infrastructure.Readers;

public sealed class TransportersInGeofence(IApplicationDbContext context) : ITransportersInGeofence
{

    public async Task<IReadOnlyCollection<TransporterInGeofenceVm>> GetTransportersInGeofencesAsync(Guid accountId, Guid? scopeUserId, Guid? geofenceId, short? type, CancellationToken cancellationToken)
    {
        var pairs = from geofence in context.Geofences
                    from transporter in context.Transporters
                    where geofence.AccountId == accountId && geofence.Active && transporter.AccountId == accountId
                    where geofenceId == null || geofence.GeofenceId == geofenceId
                    where type == null || geofence.Type == type
                    where geofence.Geom.Intersects(transporter.Geom)
                    select new { geofence, transporter };

        // Group visibility as an EXISTS predicate, never a join: the view repeats a
        // (user, transporter) pair once per shared group and a join would multiply the rows.
        if (scopeUserId is { } userId)
            pairs = pairs.Where(p => context.VisibleTransporters.Any(v =>
                v.AccountId == accountId
                && v.UserId == userId
                && v.TransporterId == p.transporter.TransporterId));

        return await pairs
            .Select(p => new TransporterInGeofenceVm
            {
                GeofenceId = p.geofence.GeofenceId,
                GeofenceName = p.geofence.Name,
                TransporterId = p.transporter.TransporterId,
                TransporterName = p.transporter.Name
            })
            .ToListAsync(cancellationToken);
    }

}

