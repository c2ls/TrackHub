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

using FluentValidation.TestHelper;
using TrackHub.Geofencing.Application.Geofences.Commands.Create;
using TrackHub.Geofencing.Domain.Models;
using TrackHub.Geofencing.Domain.Records;

namespace Application.UnitTests.Geofences;

[TestFixture]
public class CreateGeofenceValidatorTests
{
    private CreateGeofenceValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateGeofenceValidator();
    }

    // A closed, non-self-intersecting square ring (lat/lng).
    private static MultiPolygonVm ValidSquare() => new(
    [
        new CoordinateVm(0, 0),
        new CoordinateVm(0, 1),
        new CoordinateVm(1, 1),
        new CoordinateVm(1, 0),
        new CoordinateVm(0, 0),
    ], 4326);

    private static GeofenceDto PolygonDto(string name = "Warehouse Zone", MultiPolygonVm? geom = null, int? dwell = null)
        => new(Guid.NewGuid(), geom ?? ValidSquare(), name, "desc", 1, 1, true, null, null, false, false, dwell);

    private static GeofenceDto CircleDto(double radiusMeters = 250)
        => new(Guid.NewGuid(), null, "Circle Zone", null, 1, 1, true, new CoordinateVm(10, 20), radiusMeters, false, false, null);

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateGeofenceCommand(PolygonDto(name: ""));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.Name);
    }

    [Test]
    public void Should_Have_Error_When_Geofence_Is_Default()
    {
        var command = new CreateGeofenceCommand(default);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence);
    }

    [Test]
    public void Should_Have_Error_When_No_Shape_Provided()
    {
        // Neither polygon nor a complete circle.
        var dto = new GeofenceDto(Guid.NewGuid(), null, "Name", null, 1, 1, true, null, null, false, false, null);
        var command = new CreateGeofenceCommand(dto);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.Geom);
    }

    [Test]
    public void Should_Have_Error_When_Both_Circle_And_Polygon_Provided()
    {
        var dto = new GeofenceDto(Guid.NewGuid(), ValidSquare(), "Name", null, 1, 1, true, new CoordinateVm(10, 20), 250, false, false, null);
        var command = new CreateGeofenceCommand(dto);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.Geom);
    }

    [Test]
    public void Should_Have_Error_When_Polygon_Has_Too_Few_Points()
    {
        var geom = new MultiPolygonVm([new CoordinateVm(0, 0), new CoordinateVm(1, 1)], 4326);
        var command = new CreateGeofenceCommand(PolygonDto(geom: geom));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.Geom);
    }

    [Test]
    public void Should_Have_Error_When_Polygon_Self_Intersects()
    {
        // Bow-tie ring: crosses itself, rejected by NTS IsValidOp.
        var geom = new MultiPolygonVm(
        [
            new CoordinateVm(0, 0),
            new CoordinateVm(1, 1),
            new CoordinateVm(1, 0),
            new CoordinateVm(0, 1),
            new CoordinateVm(0, 0),
        ], 4326);
        var command = new CreateGeofenceCommand(PolygonDto(geom: geom));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.Geom);
    }

    [Test]
    public void Should_Have_Error_When_Circle_Radius_Too_Small()
    {
        var command = new CreateGeofenceCommand(CircleDto(radiusMeters: 5));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.CircleRadiusMeters!.Value);
    }

    [Test]
    public void Should_Have_Error_When_Circle_Radius_Too_Large()
    {
        var command = new CreateGeofenceCommand(CircleDto(radiusMeters: 100_001));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.CircleRadiusMeters!.Value);
    }

    [Test]
    public void Should_Have_Error_When_Circle_Center_Beyond_Polar_Limit()
    {
        var command = new CreateGeofenceCommand(CircleDto() with
        {
            CircleCenter = new CoordinateVm(86.5, 10.0),
        });
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.CircleCenter!.Value.Latitude);
    }

    [Test]
    public void Should_Have_Error_When_Circle_Crosses_Antimeridian()
    {
        // 50 km radius at the equator spans ~0.45°; a center at 179.9° pushes the ring past 180°.
        var command = new CreateGeofenceCommand(CircleDto(radiusMeters: 50_000) with
        {
            CircleCenter = new CoordinateVm(0.0, 179.9),
        });
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Geofence.CircleCenter");
    }

    [Test]
    public void Should_Have_Error_When_Polygon_Coordinates_Out_Of_Range()
    {
        var geom = new MultiPolygonVm(
        [
            new CoordinateVm(0, 0),
            new CoordinateVm(0, 181),
            new CoordinateVm(1, 181),
            new CoordinateVm(1, 0),
            new CoordinateVm(0, 0),
        ], 4326);
        var command = new CreateGeofenceCommand(PolygonDto(geom: geom));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.Geom);
    }

    [Test]
    public void Should_Have_Error_When_DwellThreshold_Out_Of_Range()
    {
        var command = new CreateGeofenceCommand(PolygonDto(dwell: 0));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.DwellThresholdMinutes!.Value);
    }

    [Test]
    public void Should_Have_Error_When_DwellThreshold_Above_Max()
    {
        var command = new CreateGeofenceCommand(PolygonDto(dwell: 10_081));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.DwellThresholdMinutes!.Value);
    }

    [Test]
    public void Should_Not_Have_Errors_When_Valid_Polygon()
    {
        var command = new CreateGeofenceCommand(PolygonDto(dwell: 60));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_Not_Have_Errors_When_Valid_Circle()
    {
        var command = new CreateGeofenceCommand(CircleDto());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
