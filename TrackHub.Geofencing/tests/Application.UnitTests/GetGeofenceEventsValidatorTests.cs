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
using TrackHub.Geofencing.Application.GeofenceEvents.Queries.Get;

namespace Application.UnitTests.GeofenceEvents;

[TestFixture]
public class GetGeofenceEventsValidatorTests
{
    private GetGeofenceEventsValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new GetGeofenceEventsValidator();
    }

    private static GetGeofenceEventsQuery Query(DateTimeOffset from, DateTimeOffset to)
        => new(from, to, null, null, null, null, null);

    [Test]
    public void Should_Have_Error_When_To_Equals_From()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.TestValidate(Query(now, now));
        result.ShouldHaveValidationErrorFor(v => v.To);
    }

    [Test]
    public void Should_Have_Error_When_To_Before_From()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.TestValidate(Query(now, now.AddDays(-1)));
        result.ShouldHaveValidationErrorFor(v => v.To);
    }

    [Test]
    public void Should_Have_Error_When_Window_Exceeds_MaxDays()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-100);
        var to = DateTimeOffset.UtcNow;
        var result = _validator.TestValidate(Query(from, to));
        result.ShouldHaveValidationErrorFor(v => v.To);
    }

    [Test]
    public void Should_Not_Have_Errors_When_Window_Valid()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-30);
        var to = DateTimeOffset.UtcNow;
        var result = _validator.TestValidate(Query(from, to));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
