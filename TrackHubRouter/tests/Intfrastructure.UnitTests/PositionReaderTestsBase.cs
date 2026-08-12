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

namespace TrackHub.Router.Infrastructure.Tests;

/// <summary>
/// Base class for PositionReader unit tests providing common setup and helper methods
/// </summary>
/// <typeparam name="TPositionReader">The type of PositionReader being tested</typeparam>
public abstract class PositionReaderTestsBase<TPositionReader>
{
    protected Mock<ICredentialHttpClientFactory> HttpClientFactoryMock { get; private set; } = null!;
    protected Mock<IHttpClientService> HttpClientServiceMock { get; private set; } = null!;
    // Default mock: TryGet misses (out param stays default), so readers behave as before caching.
    protected Mock<IProviderSessionStore> SessionStoreMock { get; private set; } = null!;
    protected TPositionReader PositionReader { get; private set; } = default!;
    protected CancellationToken TestCancellationToken { get; } = CancellationToken.None;

    /// <summary>
    /// Override this method to create the specific PositionReader instance
    /// </summary>
    protected abstract TPositionReader CreatePositionReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService);

    [SetUp]
    public void BaseSetUp()
    {
        HttpClientFactoryMock = new Mock<ICredentialHttpClientFactory>();
        HttpClientServiceMock = new Mock<IHttpClientService>();
        SessionStoreMock = new Mock<IProviderSessionStore>();
        PositionReader = CreatePositionReader(HttpClientFactoryMock.Object, HttpClientServiceMock.Object);
    }

    /// <summary>
    /// Creates a test DeviceTransporterVm with the specified identifier and name
    /// </summary>
    protected DeviceTransporterVm CreateDeviceTransporterVm(
        int identifier = 1, 
        string name = "TestDevice",
        string serial = "",
        Guid? transporterId = null)
        => new()
        {
            TransporterId = transporterId ?? Guid.NewGuid(),
            Identifier = identifier,
            Name = name,
            Serial = serial,
            TransporterType = "Truck",
            TransporterTypeId = (short)TransporterType.Truck
        };

    /// <summary>
    /// Creates a list of test DeviceTransporterVm instances
    /// </summary>
    protected List<DeviceTransporterVm> CreateDeviceTransporterVmList(params int[] identifiers)
        => identifiers.Select((id, index) => CreateDeviceTransporterVm(id, $"TestDevice{index + 1}")).ToList();

    /// <summary>
    /// Creates an expected PositionVm with default values for testing
    /// </summary>
    protected PositionVm CreateExpectedPositionVm(
        Guid? transporterId = null,
        string deviceName = "TestDevice",
        string transporterType = "Truck",
        double latitude = 0,
        double longitude = 0,
        double? altitude = null,
        DateTimeOffset? deviceDateTime = null,
        DateTimeOffset? serverDateTime = null,
        double speed = 0,
        double? course = null,
        int? eventId = null,
        string? address = null,
        string? city = null,
        string? state = null,
        string? country = null,
        AttributesVm? attributes = null)
        => new(
            transporterId ?? Guid.Empty,
            deviceName,
            transporterType,
            latitude,
            longitude,
            altitude,
            deviceDateTime ?? DateTimeOffset.MinValue,
            serverDateTime,
            speed,
            course,
            eventId,
            address,
            city,
            state,
            country,
            attributes
        );

    /// <summary>
    /// Asserts that the result is an empty collection
    /// </summary>
    protected void AssertIsEmpty<T>(IEnumerable<T> result)
    {
        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Asserts that the result is not empty
    /// </summary>
    protected void AssertIsNotEmpty<T>(IEnumerable<T> result)
    {
        Assert.That(result, Is.Not.Empty);
    }

    /// <summary>
    /// Asserts that the result equals the expected value
    /// For PositionVm specifically, compares individual properties to handle struct equality properly
    /// </summary>
    protected void AssertEquals<T>(T result, T expected)
    {
        if (typeof(T) == typeof(PositionVm))
        {
            var resultPosition = (PositionVm)(object)result!;
            var expectedPosition = (PositionVm)(object)expected!;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(resultPosition.TransporterId, Is.EqualTo(expectedPosition.TransporterId), "TransporterId mismatch");
                Assert.That(resultPosition.DeviceName ?? string.Empty, Is.EqualTo(expectedPosition.DeviceName ?? string.Empty), "DeviceName mismatch");
                Assert.That(resultPosition.TransporterType ?? string.Empty, Is.EqualTo(expectedPosition.TransporterType ?? string.Empty), "TransporterType mismatch");
                Assert.That(resultPosition.Latitude, Is.EqualTo(expectedPosition.Latitude), "Latitude mismatch");
                Assert.That(resultPosition.Longitude, Is.EqualTo(expectedPosition.Longitude), "Longitude mismatch");
                Assert.That(resultPosition.Altitude, Is.EqualTo(expectedPosition.Altitude), "Altitude mismatch");
                Assert.That(resultPosition.DeviceDateTime, Is.EqualTo(expectedPosition.DeviceDateTime), "DeviceDateTime mismatch");
                Assert.That(resultPosition.Speed, Is.EqualTo(expectedPosition.Speed), "Speed mismatch");
            }
        }
        else
        {
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
