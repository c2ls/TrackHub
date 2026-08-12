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

using TrackHub.Router.Infrastructure.Navixy.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Navixy.Tests;

[TestFixture]
public class DeviceReaderTests : DeviceReaderTestsBase<DeviceReader>
{
    protected override DeviceReader CreateDeviceReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService, SessionStoreMock.Object);

    [Test]
    public async Task GetDeviceAsync_WithValidDeviceDto_ReturnsDeviceVm()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1);
        var tracker = new Tracker(1, "IMEI123", "Device1", null);
        var response = new TrackerListResponse(true, [tracker]);
        var expectedDeviceVm = CreateExpectedDeviceVm(1, "IMEI123", "Device1");

        HttpClientServiceMock.Setup(x => x.PostAsync<TrackerListResponse>(It.IsAny<string>(), It.IsAny<object>(), TestCancellationToken))
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
        var devices = CreateDeviceTransporterVmList(1, 2);
        var tracker1 = new Tracker(1, "IMEI123", "Device1", null);
        var tracker2 = new Tracker(2, "IMEI456", "Device2", null);
        var response = new TrackerListResponse(true, [tracker1, tracker2]);

        HttpClientServiceMock.Setup(x => x.PostAsync<TrackerListResponse>(It.IsAny<string>(), It.IsAny<object>(), TestCancellationToken))
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
        var devices = CreateDeviceTransporterVmList(1, 2);
        var response = new TrackerListResponse(true, null);

        HttpClientServiceMock.Setup(x => x.PostAsync<TrackerListResponse>(It.IsAny<string>(), It.IsAny<object>(), TestCancellationToken))
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
        var response = new TrackerListResponse(true, []);

        HttpClientServiceMock.Setup(x => x.PostAsync<TrackerListResponse>(It.IsAny<string>(), It.IsAny<object>(), TestCancellationToken))
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
        var response = new TrackerListResponse(true, null);

        HttpClientServiceMock.Setup(x => x.PostAsync<TrackerListResponse>(It.IsAny<string>(), It.IsAny<object>(), TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await DeviceReader.GetDevicesAsync(TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }
}
