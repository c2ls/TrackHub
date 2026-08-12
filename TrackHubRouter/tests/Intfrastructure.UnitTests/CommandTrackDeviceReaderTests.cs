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
using TrackHub.Router.Infrastructure.CommandTrack.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.CommandTrack.Tests;

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
        var deviceDto = CreateDeviceTransporterVm(1);
        var devicePosition = new DevicePosition();
        var expectedDeviceVm = CreateExpectedDeviceVm(0, null, null, Guid.Empty);

        HttpClientServiceMock.Setup(x => x.GetAsync<DevicePosition>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(devicePosition);

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
        var devicePositions = new List<DevicePosition>();
        var expectedDeviceVms = new List<DeviceVm>();

        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<DevicePosition>>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(devicePositions);

        // Act
        var result = await DeviceReader.GetDevicesAsync(devices, TestCancellationToken);

        // Assert
        AssertEquals(result, expectedDeviceVms);
    }

    [Test]
    public async Task GetDevicesAsync_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var devices = CreateDeviceTransporterVmList(1, 2);
        var emptyDevices = new List<DevicePosition>();

        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<DevicePosition>>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(emptyDevices);

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
        var devicePositions = new List<DevicePosition>();

        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<DevicePosition>>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(devicePositions);

        // Act
        var result = await DeviceReader.GetDevicesAsync(devices, TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }

    [Test]
    public async Task GetDevicesAsync_WithNoPositions_ReturnsEmptyList()
    {
        // Arrange
        var emptyDevices = new List<DevicePosition>();
        
        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<DevicePosition>>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(emptyDevices);

        // Act
        var result = await DeviceReader.GetDevicesAsync(TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }
}

