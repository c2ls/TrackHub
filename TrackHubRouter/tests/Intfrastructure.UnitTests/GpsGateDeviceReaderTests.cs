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

using TrackHub.Router.Infrastructure.GpsGate.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.GpsGate.Tests;

[TestFixture]
public class DeviceReaderTests : DeviceReaderTestsBase<DeviceReader>
{
    protected override DeviceReader CreateDeviceReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService);

    [Test]
    public async Task GetDeviceAsync_WithValidDeviceDto_ReturnsDeviceVm()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1);
        var device = new Device(1, "Device1", "IMEI123", 0, 0, null, null);
        var expectedDeviceVm = CreateExpectedDeviceVm(1, "IMEI123", "Device1");

        HttpClientServiceMock.Setup(x => x.GetAsync<Device>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(device);

        // Act
        var result = await DeviceReader.GetDeviceAsync(deviceDto, TestCancellationToken);

        // Assert
        AssertEquals(result, expectedDeviceVm);
    }

    [Test]
    public async Task GetDevicesAsync_WithValidDevices_ReturnsListOfDeviceVm()
    {
        // Arrange
        var devices = CreateDeviceTransporterVmList(1, 2);
        var device1 = new Device(1, "Device1", "IMEI123", 0, 0, null, null);
        var device2 = new Device(2, "Device2", "IMEI456", 0, 0, null, null);
        var deviceResults = new List<Device> { device1, device2 };

        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<Device>>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(deviceResults);

        // Act
        var result = await DeviceReader.GetDevicesAsync(devices, TestCancellationToken);

        // Assert
        AssertIsNotEmpty(result);
    }

    [Test]
    public async Task GetDevicesAsync_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var devices = CreateDeviceTransporterVmList(1, 2);

        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<Device>>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync((IEnumerable<Device>?)null);

        // Act
        var result = await DeviceReader.GetDevicesAsync(devices, TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }

    [Test]
    public async Task GetDevicesAsync_WithNoDevices_ReturnsEmptyList()
    {
        // Arrange
        var devices = new List<DeviceTransporterVm>();
        var deviceResults = new List<Device>();

        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<Device>>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(deviceResults);

        // Act
        var result = await DeviceReader.GetDevicesAsync(devices, TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }

    [Test]
    public async Task GetDevicesAsync_WithNoPositions_ReturnsEmptyList()
    {
        // Arrange
        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<Device>>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync((IEnumerable<Device>?)null);

        // Act
        var result = await DeviceReader.GetDevicesAsync(TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }
}
