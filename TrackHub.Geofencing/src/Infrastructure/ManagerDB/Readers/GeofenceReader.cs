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

using NetTopologySuite.Geometries;

namespace TrackHub.Geofencing.Infrastructure.Readers;

public sealed class GeofenceReader(IApplicationDbContext context) : IGeofenceReader
{

    public async Task<GeofenceVm> GetGeofenceAsync(Guid id, CancellationToken cancellationToken)
    {
        var geofence = await context.Geofences
            .Where(a => a.GeofenceId.Equals(id))
            .FirstAsync(cancellationToken);

        return CastGeofence(geofence);
    }

    public async Task<GeofencesPageVm> GetGeofencesPageAsync(
        Guid accountId,
        int skip,
        int take,
        short? type,
        bool? active,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = context.Geofences
            .Where(g => g.AccountId == accountId);

        if (type is not null)
            query = query.Where(g => g.Type == type);

        if (active is not null)
            query = query.Where(g => g.Active == active);

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Escape LIKE metacharacters so a literal '%'/'_' in the search text matches itself.
            var escaped = search.Trim()
                .Replace(@"\", @"\\")
                .Replace("%", @"\%")
                .Replace("_", @"\_");
            query = query.Where(g => EF.Functions.ILike(g.Name, $"%{escaped}%", @"\"));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var geofences = await query
            .OrderBy(g => g.Name)
            .ThenBy(g => g.GeofenceId)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new GeofencesPageVm([.. geofences.Select(CastGeofence)], totalCount);
    }

    public async Task<IReadOnlyDictionary<Guid, GeofenceAlertInfoVm>> GetActiveGeofenceAlertInfoAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var infos = await context.Geofences
            .Where(g => g.AccountId == accountId && g.Active)
            .Select(g => new GeofenceAlertInfoVm(g.GeofenceId, g.Name, g.Type, g.AlertOnEntry, g.AlertOnExit))
            .ToListAsync(cancellationToken);

        return infos.ToDictionary(i => i.GeofenceId);
    }

    public async Task<IReadOnlyCollection<Guid>> GetGeofenceIdsContainingPointAsync(
        Guid accountId,
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        // Create a point geometry using NetTopologySuite
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var point = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

        // Use NTS Contains for spatial query - this translates to PostGIS ST_Contains
        var geofenceIds = await context.Geofences
            .Where(g => g.AccountId == accountId && g.Active)
            .Where(g => g.Geom.Contains(point))
            .Select(g => g.GeofenceId)
            .ToListAsync(cancellationToken);

        return geofenceIds;
    }

    private static GeofenceVm CastGeofence(Entities.Geofence geofence)
        => new(geofence.GeofenceId,
            geofence.AccountId,
            CastGeofence(geofence.Geom),
            geofence.Name,
            geofence.Description,
            geofence.Color,
            geofence.Type,
            geofence.Active,
            geofence.CircleCenter is null ? null : new CoordinateVm(geofence.CircleCenter.Y, geofence.CircleCenter.X),
            geofence.CircleRadiusMeters,
            geofence.AlertOnEntry,
            geofence.AlertOnExit,
            geofence.DwellThresholdMinutes);

    private static MultiPolygonVm CastGeofence(Polygon polygon)
    {
        var points = new List<CoordinateVm>();
        foreach (var coordinate in polygon.Coordinates)
        {
            points.Add(new CoordinateVm(coordinate.Y, coordinate.X));
        }
        return new MultiPolygonVm(points, 4326);
    }
}
