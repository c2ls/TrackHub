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
using TrackHub.Geofencing.Application.Geofences.Commands.Update;
using TrackHub.Geofencing.Application.Transporters.Commands.Update;
using TrackHub.Geofencing.Domain.Models;
using TrackHub.Geofencing.Domain.Records;

namespace Application.UnitTests.Geofences;

[TestFixture]
public class UpdateGeofenceValidatorTests
{
    private UpdateGeofenceValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateGeofenceValidator();
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

    private static GeofenceDto PolygonDto(Guid? id = null, string name = "Updated Zone", MultiPolygonVm? geom = null)
        => new(id ?? Guid.NewGuid(), geom ?? ValidSquare(), name, "desc", 2, 1, true, null, null, false, false, null);

    [Test]
    public void Should_Have_Error_When_GeofenceId_Is_Empty()
    {
        var command = new UpdateGeofenceCommand(PolygonDto(id: Guid.Empty));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.GeofenceId);
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new UpdateGeofenceCommand(PolygonDto(name: ""));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.Name);
    }

    [Test]
    public void Should_Have_Error_When_No_Shape_Provided()
    {
        var dto = new GeofenceDto(Guid.NewGuid(), null, "Name", null, 1, 1, true, null, null, false, false, null);
        var command = new UpdateGeofenceCommand(dto);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.Geom);
    }

    [Test]
    public void Should_Have_Error_When_Polygon_Self_Intersects()
    {
        var geom = new MultiPolygonVm(
        [
            new CoordinateVm(0, 0),
            new CoordinateVm(1, 1),
            new CoordinateVm(1, 0),
            new CoordinateVm(0, 1),
            new CoordinateVm(0, 0),
        ], 4326);
        var command = new UpdateGeofenceCommand(PolygonDto(geom: geom));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence.Geom);
    }

    [Test]
    public void Should_Have_Error_When_Geofence_Is_Default()
    {
        var command = new UpdateGeofenceCommand(default);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Geofence);
    }

    [Test]
    public void Should_Not_Have_Errors_When_Valid()
    {
        var command = new UpdateGeofenceCommand(PolygonDto());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
