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
public class PositionReaderTests : PositionReaderTestsBase<PositionReader>
{
    private Mock<ICredentialWriter> _credentialWriterMock = null!;

    protected override PositionReader CreatePositionReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
    {
        _credentialWriterMock = new Mock<ICredentialWriter>();
        return new PositionReader(httpClientFactory, httpClientService, _credentialWriterMock.Object, SessionStoreMock.Object);
    }

    [Test]
    public async Task GetDevicePositionAsync_WithValidDeviceDto_ReturnsPositionVm()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice");
        var devicePosition = new DevicePosition();

        HttpClientServiceMock.Setup(x => x.GetAsync<DevicePosition>(
            It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(devicePosition);

        // Act
        var result = await PositionReader.GetDevicePositionAsync(deviceDto, TestCancellationToken);

        // Assert
        Assert.That(result.TransporterId, Is.EqualTo(deviceDto.TransporterId));
    }

    [Test]
    public async Task GetDevicePositionAsync_WithMultipleDevices_ReturnsPositionVmList()
    {
        // Arrange
        var devices = CreateDeviceTransporterVmList(1, 2);
        var positions = new List<DevicePosition>();

        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<DevicePosition>>(
            It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(positions);

        // Act
        var result = await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }

    [Test]
    public async Task GetDevicePositionAsync_WithNullResponse_ReturnsEmptyList()
    {
        // Arrange
        var devices = CreateDeviceTransporterVmList(1, 2);

        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<DevicePosition>>(
            It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync((IEnumerable<DevicePosition>?)null);

        // Act
        var result = await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }

    [Test]
    public async Task GetPositionAsync_WithDateRange_ReturnsPositionVmList()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice");
        var from = DateTimeOffset.Now.AddHours(-1);
        var to = DateTimeOffset.Now;
        var positions = new List<Position>();

        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<Position>>(
            It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(positions);

        // Act
        var result = await PositionReader.GetPositionAsync(from, to, deviceDto, TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }

    [Test]
    public async Task GetPositionAsync_WithNullResponse_ReturnsEmptyList()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice");
        var from = DateTimeOffset.Now.AddHours(-1);
        var to = DateTimeOffset.Now;

        HttpClientServiceMock.Setup(x => x.GetAsync<IEnumerable<Position>>(
            It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync((IEnumerable<Position>?)null);

        // Act
        var result = await PositionReader.GetPositionAsync(from, to, deviceDto, TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }
}
