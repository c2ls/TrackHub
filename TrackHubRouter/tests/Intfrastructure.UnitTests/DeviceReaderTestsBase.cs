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

global using Moq;
global using TrackHub.Router.Domain.Interfaces;
global using TrackHub.Router.Domain.Models;
global using Common.Domain.Enums;

namespace TrackHub.Router.Infrastructure.Tests;

/// <summary>
/// Base class for DeviceReader unit tests providing common setup and helper methods
/// </summary>
/// <typeparam name="TDeviceReader">The type of DeviceReader being tested</typeparam>
public abstract class DeviceReaderTestsBase<TDeviceReader>
{
    protected Mock<ICredentialHttpClientFactory> HttpClientFactoryMock { get; private set; } = null!;
    protected Mock<IHttpClientService> HttpClientServiceMock { get; private set; } = null!;
    // Default mock: TryGet misses (out param stays default), so readers behave as before caching.
    protected Mock<IProviderSessionStore> SessionStoreMock { get; private set; } = null!;
    protected TDeviceReader DeviceReader { get; private set; } = default!;
    protected CancellationToken TestCancellationToken { get; } = CancellationToken.None;

    /// <summary>
    /// Override this method to create the specific DeviceReader instance
    /// </summary>
    protected abstract TDeviceReader CreateDeviceReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService);

    [SetUp]
    public void BaseSetUp()
    {
        HttpClientFactoryMock = new Mock<ICredentialHttpClientFactory>();
        HttpClientServiceMock = new Mock<IHttpClientService>();
        SessionStoreMock = new Mock<IProviderSessionStore>();
        DeviceReader = CreateDeviceReader(HttpClientFactoryMock.Object, HttpClientServiceMock.Object);
    }

    /// <summary>
    /// Creates a test DeviceTransporterVm with the specified identifier
    /// </summary>
    protected DeviceTransporterVm CreateDeviceTransporterVm(int identifier, string serial = "")
        => new()
        {
            Identifier = identifier,
            Serial = serial
        };

    /// <summary>
    /// Creates a list of test DeviceTransporterVm instances
    /// </summary>
    protected List<DeviceTransporterVm> CreateDeviceTransporterVmList(params int[] identifiers)
        => identifiers.Select(id => CreateDeviceTransporterVm(id)).ToList();

    /// <summary>
    /// Creates an expected DeviceVm with default values for testing.
    /// Note: Uses Guid.Empty instead of null for DeviceId and null for strings to match mapper behavior with default structs
    /// </summary>
    protected DeviceVm CreateExpectedDeviceVm(
        int identifier = 0,
        string? serial = null,
        string? name = null,
        Guid? deviceId = null)
        => new(
            deviceId ?? Guid.Empty,
            identifier,
            serial ?? string.Empty,
            name ?? string.Empty,
            (short)DeviceType.Cellular,
            (short)TransporterType.Truck
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
    /// For DeviceVm specifically, compares individual properties to handle struct equality properly
    /// </summary>
    protected void AssertEquals<T>(T result, T expected)
    {
        if (typeof(T) == typeof(DeviceVm))
        {
            var resultDevice = (DeviceVm)(object)result!;
            var expectedDevice = (DeviceVm)(object)expected!;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(resultDevice.DeviceId, Is.EqualTo(expectedDevice.DeviceId), "DeviceId mismatch");
                Assert.That(resultDevice.Identifier, Is.EqualTo(expectedDevice.Identifier), "Identifier mismatch");
                Assert.That(resultDevice.Serial ?? string.Empty, Is.EqualTo(expectedDevice.Serial ?? string.Empty), "Serial mismatch");
                Assert.That(resultDevice.Name ?? string.Empty, Is.EqualTo(expectedDevice.Name ?? string.Empty), "Name mismatch");
                Assert.That(resultDevice.DeviceTypeId, Is.EqualTo(expectedDevice.DeviceTypeId), "DeviceTypeId mismatch");
                Assert.That(resultDevice.TransporterTypeId, Is.EqualTo(expectedDevice.TransporterTypeId), "TransporterTypeId mismatch");
            }
        }
        else
        {
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
