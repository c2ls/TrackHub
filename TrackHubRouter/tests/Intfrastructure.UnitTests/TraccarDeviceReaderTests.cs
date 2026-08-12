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

using TrackHub.Router.Infrastructure.Traccar.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Traccar.Tests;

[TestFixture]
public class DeviceReaderTests : DeviceReaderTestsBase<DeviceReader>
{
    protected override DeviceReader CreateDeviceReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService);

    [Test]
    public async Task GetDeviceAsync_ValidDevice_ReturnsDeviceVm()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1);
        var device = new Device();
        var expectedDeviceVm = CreateExpectedDeviceVm(0, null, null, Guid.Empty);

        HttpClientServiceMock.Setup(x => x.GetAsync<Device>("api/devices?id=1", null, TestCancellationToken))
            .ReturnsAsync(device);

        // Act
        var result = await DeviceReader.GetDeviceAsync(deviceDto, TestCancellationToken);

        // Assert
        AssertEquals(result, expectedDeviceVm);
    }

    [Test]
    public async Task GetDevicesAsync_ValidDevices_ReturnsDeviceVms()
    {
        // Arrange
        var devices = CreateDeviceTransporterVmList(1, 2);
        var resultDevices = new List<Device>();
        var expectedDeviceVms = new List<DeviceVm>();

        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<Device>>("api/devices?id=1,2", null, TestCancellationToken))
            .ReturnsAsync(resultDevices);

        // Act
        var result = await DeviceReader.GetDevicesAsync(devices, TestCancellationToken);

        // Assert
        AssertEquals(result, expectedDeviceVms);
    }

    [Test]
    public async Task GetDevicesAsync_NoDevices_ReturnsEmptyList()
    {
        // Arrange
        var positions = new List<Device>();

        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<Device>>("api/devices?all=true", null, TestCancellationToken))
            .ReturnsAsync(positions);

        // Act
        var result = await DeviceReader.GetDevicesAsync(TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }
}

