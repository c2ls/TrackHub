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

using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Infrastructure.Protrack.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Protrack.Tests;

[TestFixture]
public class DeviceReaderTests : DeviceReaderTestsBase<DeviceReader>
{
    private Mock<ICredentialWriter> _credentialWriterMock = null!;

    protected override DeviceReader CreateDeviceReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
    {
        _credentialWriterMock = new Mock<ICredentialWriter>();
        return new DeviceReader(httpClientFactory, httpClientService, _credentialWriterMock.Object, SessionStoreMock.Object);
    }

    [Test]
    public async Task GetDeviceAsync_WithValidDeviceDto_ReturnsDeviceVm()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "IMEI123");
        var deviceRecord = new DeviceRecord("IMEI123", "Device1", "VT05S", "Plate1", 0, 0);
        var response = new DeviceListResponse(0, [deviceRecord]);
        var expectedDeviceVm = CreateExpectedDeviceVm(1, "IMEI123", "Device1");

        HttpClientServiceMock.Setup(x => x.GetAsync<DeviceListResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await DeviceReader.GetDeviceAsync(deviceDto, TestCancellationToken);

        // Assert
        AssertEquals(result, expectedDeviceVm);
    }

    [Test]
    public void GetDeviceAsync_WithNullResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "IMEI123");
        var response = new DeviceListResponse(0, null);

        HttpClientServiceMock.Setup(x => x.GetAsync<DeviceListResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(response);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await DeviceReader.GetDeviceAsync(deviceDto, TestCancellationToken));
    }

    [Test]
    public async Task GetDevicesAsync_WithValidDevices_ReturnsListOfDeviceVm()
    {
        // Arrange
        var devices = new List<DeviceTransporterVm>
        {
            CreateDeviceTransporterVm(1, "IMEI123"),
            CreateDeviceTransporterVm(2, "IMEI456")
        };
        var deviceRecord1 = new DeviceRecord("IMEI123", "Device1", "VT05S", "Plate1", 0, 0);
        var deviceRecord2 = new DeviceRecord("IMEI456", "Device2", "VT05S", "Plate2", 0, 0);
        var response = new DeviceListResponse(0, [deviceRecord1, deviceRecord2]);

        HttpClientServiceMock.Setup(x => x.GetAsync<DeviceListResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), TestCancellationToken))
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
            CreateDeviceTransporterVm(1, "IMEI123")
        };
        var response = new DeviceListResponse(0, null);

        HttpClientServiceMock.Setup(x => x.GetAsync<DeviceListResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), TestCancellationToken))
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

        // Act
        var result = await DeviceReader.GetDevicesAsync(devices, TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }

    [Test]
    public async Task GetDevicesAsync_WithNoPositions_ReturnsEmptyList()
    {
        // Arrange
        var response = new DeviceListResponse(0, null);

        HttpClientServiceMock.Setup(x => x.GetAsync<DeviceListResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await DeviceReader.GetDevicesAsync(TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }
}
