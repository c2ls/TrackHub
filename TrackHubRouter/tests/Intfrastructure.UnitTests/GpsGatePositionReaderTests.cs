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
public class PositionReaderTests : PositionReaderTestsBase<PositionReader>
{
    protected override PositionReader CreatePositionReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService);

    [Test]
    public async Task GetDevicePositionAsync_WithValidDeviceDto_ReturnsPositionVm()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice");
        var device = new Device(1, "TestDevice", "IMEI123", 0, 0, null, null);

        HttpClientServiceMock.Setup(x => x.GetAsync<Device>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(device);

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
        var device1 = new Device(1, "TestDevice1", "IMEI123", 0, 0, null, null);
        var device2 = new Device(2, "TestDevice2", "IMEI456", 0, 0, null, null);

        HttpClientServiceMock.SetupSequence(x => x.GetAsync<Device>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(device1)
            .ReturnsAsync(device2);

        // Act
        var result = await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken);

        // Assert
        AssertIsNotEmpty(result);
    }

    [Test]
    public void GetPositionAsync_WithDateRange_ThrowsNotImplementedException()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice");
        var from = DateTimeOffset.Now.AddHours(-1);
        var to = DateTimeOffset.Now;

        // Act & Assert
        Assert.ThrowsAsync<NotImplementedException>(
            async () => await PositionReader.GetPositionAsync(from, to, deviceDto, TestCancellationToken));
    }
}
