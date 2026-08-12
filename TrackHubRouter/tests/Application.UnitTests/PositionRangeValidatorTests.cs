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
using TrackHub.Router.Application.Positions.Queries.GetRange;
using TrackHub.Router.Application.Positions.Queries.GetTrips;

namespace TrackHub.Router.Application.UnitTests.Positions.Queries;

[TestFixture]
public class PositionRangeValidatorTests
{
    private static readonly DateTimeOffset From = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private GetPositionsRecordQueryValidator _rangeValidator;
    private GetPositionTripsQueryValidator _tripsValidator;

    [SetUp]
    public void SetUp()
    {
        _rangeValidator = new GetPositionsRecordQueryValidator();
        _tripsValidator = new GetPositionTripsQueryValidator();
    }

    [Test]
    public void Range_Should_Have_Error_When_TransporterId_Is_Empty()
    {
        var result = _rangeValidator.TestValidate(new GetPositionsRecordQuery(Guid.Empty, From, From.AddHours(1)));
        result.ShouldHaveValidationErrorFor(x => x.TransporterId);
    }

    [Test]
    public void Range_Should_Have_Error_When_From_Is_Not_Earlier_Than_To()
    {
        var result = _rangeValidator.TestValidate(new GetPositionsRecordQuery(Guid.NewGuid(), From.AddHours(1), From));
        result.ShouldHaveValidationErrorFor(x => x).WithErrorMessage("From must be earlier than To.");
    }

    [Test]
    public void Range_Should_Have_Error_When_Span_Exceeds_31_Days()
    {
        var result = _rangeValidator.TestValidate(new GetPositionsRecordQuery(Guid.NewGuid(), From, From.AddDays(32)));
        result.ShouldHaveValidationErrorFor(x => x).WithErrorMessage("The requested range exceeds the maximum of 31 days.");
    }

    [Test]
    public void Range_Should_Not_Have_Error_For_A_Valid_Span()
    {
        var result = _rangeValidator.TestValidate(new GetPositionsRecordQuery(Guid.NewGuid(), From, From.AddDays(31)));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Trips_Should_Have_Error_When_TransporterId_Is_Empty()
    {
        var result = _tripsValidator.TestValidate(new GetPositionTripsQuery(Guid.Empty, From, From.AddHours(1)));
        result.ShouldHaveValidationErrorFor(x => x.TransporterId);
    }

    [Test]
    public void Trips_Should_Have_Error_When_From_Is_Not_Earlier_Than_To()
    {
        var result = _tripsValidator.TestValidate(new GetPositionTripsQuery(Guid.NewGuid(), From, From));
        result.ShouldHaveValidationErrorFor(x => x).WithErrorMessage("From must be earlier than To.");
    }

    [Test]
    public void Trips_Should_Have_Error_When_Span_Exceeds_31_Days()
    {
        var result = _tripsValidator.TestValidate(new GetPositionTripsQuery(Guid.NewGuid(), From, From.AddDays(32)));
        result.ShouldHaveValidationErrorFor(x => x).WithErrorMessage("The requested range exceeds the maximum of 31 days.");
    }

    [Test]
    public void Trips_Should_Not_Have_Error_For_A_Valid_Span()
    {
        var result = _tripsValidator.TestValidate(new GetPositionTripsQuery(Guid.NewGuid(), From, From.AddDays(31)));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
