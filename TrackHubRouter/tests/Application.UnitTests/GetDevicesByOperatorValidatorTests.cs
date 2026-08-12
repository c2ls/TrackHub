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
using TrackHub.Router.Application.Devices.Queries.GetByOperator;

namespace TrackHub.Router.Application.UnitTests.Devices.Queries.GetByOperator;

[TestFixture]
public class GetDevicesByOperatorValidatorTests
{
    private GetDevicesByOperatorValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new GetDevicesByOperatorValidator();
    }

    [Test]
    public void Should_Have_Error_When_OperatorId_Is_Empty()
    {
        var query = new GetDevicesByOperatorQuery(Guid.Empty);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.OperatorId);
    }

    [Test]
    public void Should_Not_Have_Error_When_OperatorId_Is_Valid()
    {
        var query = new GetDevicesByOperatorQuery(Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_Have_Error_When_OperatorId_Is_Default()
    {
        var query = new GetDevicesByOperatorQuery(default);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.OperatorId);
    }
}
