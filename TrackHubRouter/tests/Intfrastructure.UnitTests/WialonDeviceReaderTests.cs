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

using TrackHub.Router.Infrastructure.Wialon.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Wialon.Tests;

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
        var unit = new Unit(1, "Unit1", 2, "UID123", null);
        var response = new SingleItemResponse(unit);
        var expectedDeviceVm = CreateExpectedDeviceVm(1, "UID123", "Unit1");

        HttpClientServiceMock.Setup(x => x.PostAsync<SingleItemResponse>(It.IsAny<string>(), It.IsAny<object>(), TestCancellationToken))
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
        var unit1 = new Unit(1, "Unit1", 2, "UID123", null);
        var unit2 = new Unit(2, "Unit2", 2, "UID456", null);
        var response = new SearchResponse([unit1, unit2], 2);

        HttpClientServiceMock.Setup(x => x.PostAsync<SearchResponse>(It.IsAny<string>(), It.IsAny<object>(), TestCancellationToken))
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
        var response = new SearchResponse(null, 0);

        HttpClientServiceMock.Setup(x => x.PostAsync<SearchResponse>(It.IsAny<string>(), It.IsAny<object>(), TestCancellationToken))
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
        var response = new SearchResponse([], 0);

        HttpClientServiceMock.Setup(x => x.PostAsync<SearchResponse>(It.IsAny<string>(), It.IsAny<object>(), TestCancellationToken))
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
        var response = new SearchResponse(null, 0);

        HttpClientServiceMock.Setup(x => x.PostAsync<SearchResponse>(It.IsAny<string>(), It.IsAny<object>(), TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await DeviceReader.GetDevicesAsync(TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }
}
