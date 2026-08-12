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
using NetTopologySuite.Operation.Valid;

namespace TrackHub.Geofencing.Application.Geofences.Commands;

/// <summary>
/// Shared shape rules for create/update geofence commands: circle (center + radius) XOR polygon
/// coordinates, radius bounds, NTS ring validity, and dwell threshold bounds.
/// </summary>
public sealed class GeofenceDtoValidator : AbstractValidator<GeofenceDto>
{
    public const double MinCircleRadiusMeters = 10;
    public const double MaxCircleRadiusMeters = 100_000;
    public const int MinDwellThresholdMinutes = 1;
    public const int MaxDwellThresholdMinutes = 10_080;
    // The equirectangular circle buffer degrades near the poles and cannot represent a ring
    // crossing the ±180° meridian in planar PostGIS geometry (incoming positions are
    // normalized to [-180, 180], so a ring extending past ±180 would silently miss them).
    // Both cases are rejected explicitly instead of misdetecting.
    public const double MaxAbsoluteCircleCenterLatitude = 85;
    private const double MetersPerDegreeLatitude = 111_320d;

    public GeofenceDtoValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty();

        RuleFor(v => v)
            .Must(v => HasCompleteCircle(v) || v.CircleCenter is null && v.CircleRadiusMeters is null)
            .WithMessage("A circle geofence requires both center and radius.")
            .Must(v => HasCompleteCircle(v) ^ v.Geom is not null)
            .WithMessage("Provide either polygon coordinates or a circle (center + radius), not both.")
            .WithName(nameof(GeofenceDto.Geom));

        When(v => HasCompleteCircle(v), () =>
        {
            RuleFor(v => v.CircleRadiusMeters!.Value)
                .InclusiveBetween(MinCircleRadiusMeters, MaxCircleRadiusMeters)
                .WithName(nameof(GeofenceDto.CircleRadiusMeters));

            RuleFor(v => v.CircleCenter!.Value.Latitude)
                .InclusiveBetween(-MaxAbsoluteCircleCenterLatitude, MaxAbsoluteCircleCenterLatitude)
                .WithName(nameof(GeofenceDto.CircleCenter))
                .WithMessage($"A circle center latitude must be between -{MaxAbsoluteCircleCenterLatitude} and {MaxAbsoluteCircleCenterLatitude} degrees.");

            RuleFor(v => v.CircleCenter!.Value.Longitude)
                .InclusiveBetween(-180, 180)
                .WithName(nameof(GeofenceDto.CircleCenter));

            RuleFor(v => v)
                .Must(StaysWithinLongitudeBounds)
                .WithMessage("A circle geofence may not cross the ±180° meridian.")
                .WithName(nameof(GeofenceDto.CircleCenter));
        });

        When(v => v.Geom is not null && v.CircleCenter is null && v.CircleRadiusMeters is null, () =>
            RuleFor(v => v.Geom!.Value)
                .Custom(ValidatePolygon));

        When(v => v.DwellThresholdMinutes is not null, () =>
            RuleFor(v => v.DwellThresholdMinutes!.Value)
                .InclusiveBetween(MinDwellThresholdMinutes, MaxDwellThresholdMinutes)
                .WithName(nameof(GeofenceDto.DwellThresholdMinutes)));
    }

    private static bool HasCompleteCircle(GeofenceDto dto)
        => dto.CircleCenter is not null && dto.CircleRadiusMeters is not null;

    private static bool StaysWithinLongitudeBounds(GeofenceDto dto)
    {
        if (dto.CircleCenter is not { } center || dto.CircleRadiusMeters is not { } radiusMeters)
            return true;
        if (Math.Abs(center.Latitude) > MaxAbsoluteCircleCenterLatitude || Math.Abs(center.Longitude) > 180)
            return true; // reported by the range rules above

        var metersPerDegreeLongitude = MetersPerDegreeLatitude * Math.Cos(center.Latitude * Math.PI / 180d);
        var longitudeSpanDegrees = radiusMeters / metersPerDegreeLongitude;
        return center.Longitude - longitudeSpanDegrees >= -180
            && center.Longitude + longitudeSpanDegrees <= 180;
    }

    private static void ValidatePolygon(MultiPolygonVm geom, ValidationContext<GeofenceDto> context)
    {
        var coordinates = geom.Coordinates?
            .Select(c => new Coordinate(c.Longitude, c.Latitude))
            .ToList() ?? [];

        if (coordinates.Count > 0 && !coordinates[0].Equals2D(coordinates[^1]))
            coordinates.Add(coordinates[0].Copy());

        if (coordinates.Count < 4)
        {
            context.AddFailure(nameof(GeofenceDto.Geom), "A polygon requires at least three distinct points.");
            return;
        }

        if (coordinates.Any(c => Math.Abs(c.Y) > 90 || Math.Abs(c.X) > 180))
        {
            context.AddFailure(nameof(GeofenceDto.Geom),
                "Polygon coordinates must be within latitude [-90, 90] and longitude [-180, 180].");
            return;
        }

        Polygon polygon;
        try
        {
            polygon = new Polygon(new LinearRing([.. coordinates]))
            {
                SRID = 4326
            };
        }
        catch (ArgumentException ex)
        {
            context.AddFailure(nameof(GeofenceDto.Geom), $"Invalid polygon ring: {ex.Message}");
            return;
        }

        var validOp = new IsValidOp(polygon);
        if (!validOp.IsValid)
            context.AddFailure(nameof(GeofenceDto.Geom), $"Invalid polygon: {validOp.ValidationError}.");
    }
}
