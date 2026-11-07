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

using Common.Domain.Constants;
using FluentAssertions;
using Xunit;

namespace Common.Domain.Tests.Constants;

public class StaffTypesTests
{
    [Theory]
    [InlineData(StaffTypes.Driver, "Driver")]
    [InlineData(StaffTypes.Operator, "Operator")]
    [InlineData(StaffTypes.DriverAssistant, "DriverAssistant")]
    [InlineData(StaffTypes.Mechanic, "Mechanic")]
    [InlineData(StaffTypes.Supervisor, "Supervisor")]
    [InlineData(StaffTypes.Administrative, "Administrative")]
    [InlineData(StaffTypes.Other, "Other")]
    public void StaffTypes_HaveExpectedValues(string actual, string expected) =>
        actual.Should().Be(expected);

    [Fact]
    public void Default_IsDriver_AndEveryCoreTypeIsValid()
    {
        StaffTypes.Default.Should().Be(StaffTypes.Driver);
        StaffTypes.All.Should().Contain([StaffTypes.Driver, StaffTypes.Operator, StaffTypes.DriverAssistant, StaffTypes.Mechanic, StaffTypes.Supervisor, StaffTypes.Administrative, StaffTypes.Other]);
        StaffTypes.IsValid("Driver").Should().BeTrue();
        StaffTypes.IsValid("driver").Should().BeFalse();
        StaffTypes.IsValid(null).Should().BeFalse();
    }

    [Theory]
    [InlineData(StaffCapabilities.Vehicle, "Vehicle")]
    [InlineData(StaffCapabilities.Status, "Status")]
    [InlineData(StaffCapabilities.Capture, "Capture")]
    [InlineData(StaffCapabilities.Trips, "Trips")]
    public void StaffCapabilities_HaveExpectedValues(string actual, string expected) =>
        actual.Should().Be(expected);

    [Fact]
    public void Defaults_OnlyVehicleTypesCarryTheVehicleCapability()
    {
        StaffTypeCapabilityDefaults.For(StaffTypes.Driver).Should().Contain([StaffCapabilities.Vehicle, StaffCapabilities.Status, StaffCapabilities.Capture, StaffCapabilities.Trips]);
        StaffTypeCapabilityDefaults.For(StaffTypes.Operator).Should().Contain(StaffCapabilities.Vehicle).And.NotContain(StaffCapabilities.Trips);
        foreach (var type in new[] { StaffTypes.DriverAssistant, StaffTypes.Mechanic, StaffTypes.Supervisor, StaffTypes.Administrative, StaffTypes.Other })
        {
            StaffTypeCapabilityDefaults.For(type).Should().NotContain(StaffCapabilities.Vehicle).And.Contain(StaffCapabilities.Status);
        }

        StaffTypeCapabilityDefaults.For("Unknown").Should().BeEmpty();
    }

    [Fact]
    public void Features_GateEachCoreCapabilityOnItsOwningFeature()
    {
        StaffCapabilityFeatures.FeatureKeyFor(StaffCapabilities.Vehicle).Should().Be(FeatureKeys.StaffMobile);
        StaffCapabilityFeatures.FeatureKeyFor(StaffCapabilities.Trips).Should().Be(FeatureKeys.TripManagement);
        StaffCapabilityFeatures.FeatureKeyFor("Unknown").Should().BeNull();
        StaffCapabilityFeatures.Available([StaffCapabilities.Vehicle, StaffCapabilities.Capture, StaffCapabilities.Trips], [FeatureKeys.StaffMobile, FeatureKeys.Documents])
            .Should().BeEquivalentTo([StaffCapabilities.Vehicle, StaffCapabilities.Capture]);
    }
}
