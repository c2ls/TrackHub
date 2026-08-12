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
using TrackHub.Manager.Application.Transporters.Commands.Create;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Transporters;

[TestFixture]
public class CreateTransporterValidatorTests
{
    private CreateTransporterValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateTransporterValidator();
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateTransporterCommand(new TransporterDto("", 1, Guid.NewGuid()));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Transporter.Name);
    }

    [Test]
    public void Should_Have_Error_When_TransporterTypeId_Is_Zero()
    {
        var command = new CreateTransporterCommand(new TransporterDto("Name", 0, Guid.NewGuid()));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Transporter.TransporterTypeId);
    }

    [Test]
    public void Should_Have_Error_When_Transporter_Is_Default()
    {
        var command = new CreateTransporterCommand(default);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Transporter);
    }

    [Test]
    public void Should_Not_Have_Errors_When_Valid()
    {
        var command = new CreateTransporterCommand(new TransporterDto("Truck-001", 1, Guid.NewGuid()));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
