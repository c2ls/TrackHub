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

namespace TrackHub.TripManagement.Infrastructure.TripDB.Writers;

/// <summary>
/// Geometry construction for the trip module.
/// <para>
/// Metre-radius buffers use the same equirectangular approximation as Geofencing's circle
/// geofences (<c>GeofenceWriter.BuildCirclePolygon</c>) rather than a raw <c>ST_Buffer</c> call.
/// Two reasons: it keeps the DbContext from leaking raw SQL out of this project, and PostGIS
/// <c>ST_Buffer</c> on a 4326 <c>geometry</c> buffers in DEGREES - getting metres out of it needs a
/// geography cast and a re-cast back, which produces a geometry that is no more accurate than this
/// at the radii this module allows (arrival 50-5000 m, corridor 100-5000 m).
/// </para>
/// </summary>
internal static class TripGeometryFactory
{
    private const int CircleSegments = 64;
    private const double MetersPerDegreeLatitude = 111_320d;

    internal static Point Point(double latitude, double longitude)
        => new(longitude, latitude) { SRID = TripGeometryDefaults.Srid };

    /// <summary>A stop's arrival ring: the stop point buffered by its radius.</summary>
    internal static Polygon Buffer(Point center, double radiusMeters)
        => Buffer(center.Y, center.X, radiusMeters);

    internal static Polygon Buffer(double latitude, double longitude, double radiusMeters)
    {
        var metersPerDegreeLongitude = MetersPerDegreeLatitude *
            Math.Max(Math.Cos(latitude * Math.PI / 180d), 0.01);

        var coordinates = new Coordinate[CircleSegments + 1];
        for (var i = 0; i < CircleSegments; i++)
        {
            var angle = 2d * Math.PI * i / CircleSegments;
            coordinates[i] = new Coordinate(
                longitude + (radiusMeters * Math.Sin(angle) / metersPerDegreeLongitude),
                latitude + (radiusMeters * Math.Cos(angle) / MetersPerDegreeLatitude));
        }

        coordinates[CircleSegments] = coordinates[0].Copy();

        return new Polygon(new LinearRing(coordinates)) { SRID = TripGeometryDefaults.Srid };
    }

    internal static LineString? Line(IReadOnlyCollection<CoordinateVm> coordinates)
    {
        if (coordinates.Count < 2)
        {
            return null;
        }

        return new LineString([.. coordinates.Select(c => new Coordinate(c.Longitude, c.Latitude))])
        {
            SRID = TripGeometryDefaults.Srid,
        };
    }

    /// <summary>
    /// The route corridor: the planned line buffered by <paramref name="corridorMeters"/>.
    /// <para>
    /// <b>Interior rings are preserved.</b> Buffering a route that doubles back or closes a loop
    /// produces a polygon with holes, and the enclosed area is genuinely NOT within
    /// <paramref name="corridorMeters"/> of the route. Keeping only the exterior ring fills those
    /// holes in, so on an out-and-back or ring-road route a vehicle could abandon the plan anywhere
    /// inside the loop and never trip the <c>ST_Contains</c> deviation test (spec 11 §7.4).
    /// </para>
    /// </summary>
    internal static Polygon? Corridor(LineString? line, int corridorMeters)
    {
        if (line is null || line.NumPoints < 2)
        {
            return null;
        }

        var midLatitude = line.Coordinates.Average(c => c.Y);
        var metersPerDegreeLongitude = MetersPerDegreeLatitude *
            Math.Max(Math.Cos(midLatitude * Math.PI / 180d), 0.01);

        // Buffer in degrees, scaled per axis so the corridor width is metres on both.
        var scaled = new LineString([.. line.Coordinates.Select(c =>
            new Coordinate(c.X * metersPerDegreeLongitude, c.Y * MetersPerDegreeLatitude))])
        {
            SRID = TripGeometryDefaults.Srid,
        };

        if (scaled.Buffer(corridorMeters) is not Polygon buffered)
        {
            return null;
        }

        LinearRing Unscale(LineString ring) => new([.. ring.Coordinates.Select(c =>
            new Coordinate(c.X / metersPerDegreeLongitude, c.Y / MetersPerDegreeLatitude))])
        {
            SRID = TripGeometryDefaults.Srid,
        };

        return new Polygon(
            Unscale(buffered.ExteriorRing),
            [.. buffered.InteriorRings.Select(Unscale)])
        {
            SRID = TripGeometryDefaults.Srid,
        };
    }
}
