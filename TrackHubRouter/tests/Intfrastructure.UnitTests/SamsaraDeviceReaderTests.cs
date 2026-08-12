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

using TrackHub.Router.Infrastructure.Samsara.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Samsara.Tests;

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
        var deviceDto = CreateDeviceTransporterVm(1, "ABC123");
        var vehicle = new VehicleStats("ABC123", "Vehicle1", null);
        var response = new VehicleStatsResponse([vehicle], null);
        var expectedDeviceVm = CreateExpectedDeviceVm(0, "ABC123", "Vehicle1");

        HttpClientServiceMock.Setup(x => x.GetAsync<VehicleStatsResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await DeviceReader.GetDeviceAsync(deviceDto, TestCancellationToken);

        // Assert
        AssertEquals(result, expectedDeviceVm);
    }

    [Test]
    public async Task GetDevicesAsync_WithValidDevices_ReturnsListOfDeviceVm()
    {
        // Arrange
        var devices = new List<DeviceTransporterVm> 
        { 
            CreateDeviceTransporterVm(1, "ABC123"), 
            CreateDeviceTransporterVm(2, "XYZ456") 
        };
        var vehicle1 = new VehicleStats("ABC123", "Vehicle1", null);
        var vehicle2 = new VehicleStats("XYZ456", "Vehicle2", null);
        var response = new VehicleStatsResponse([vehicle1, vehicle2], null);

        HttpClientServiceMock.Setup(x => x.GetAsync<VehicleStatsResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await DeviceReader.GetDevicesAsync(devices, TestCancellationToken);

        // Assert
        AssertIsNotEmpty(result);
    }

    [Test]
    public async Task GetDevicesAsync_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var devices = new List<DeviceTransporterVm> 
        { 
            CreateDeviceTransporterVm(1, "ABC123"), 
            CreateDeviceTransporterVm(2, "XYZ456") 
        };
        var response = new VehicleStatsResponse(null, null);

        HttpClientServiceMock.Setup(x => x.GetAsync<VehicleStatsResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(response);

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
        var response = new VehicleStatsResponse([], null);

        HttpClientServiceMock.Setup(x => x.GetAsync<VehicleStatsResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await DeviceReader.GetDevicesAsync(devices, TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }

    [Test]
    public async Task GetDevicesAsync_WithNoPositions_ReturnsEmptyList()
    {
        // Arrange
        var response = new VehicleStatsResponse(null, null);

        HttpClientServiceMock.Setup(x => x.GetAsync<VehicleStatsResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await DeviceReader.GetDevicesAsync(TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }
}
