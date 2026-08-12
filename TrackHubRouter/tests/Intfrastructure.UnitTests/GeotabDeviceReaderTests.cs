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

using TrackHub.Router.Domain.Models;
using Common.Domain.Enums;

namespace TrackHub.Router.Infrastructure.Geotab.Tests;

[TestFixture]
public class DeviceReaderTests
{
    private DeviceReader _deviceReader;

    [SetUp]
    public void Setup()
    {
        _deviceReader = new DeviceReader(new Mock<IProviderSessionStore>().Object);
    }

    [Test]
    public void GetDeviceAsync_WithNullGeotabApi_ThrowsNullReferenceException()
    {
        // Arrange
        var deviceDto = new DeviceTransporterVm { Identifier = 1 };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        Assert.ThrowsAsync<NullReferenceException>(async () => await _deviceReader.GetDeviceAsync(deviceDto, cancellationToken));
    }

    [Test]
    public void GetDevicesAsync_WithNullGeotabApi_ThrowsNullReferenceException()
    {
        // Arrange
        var devices = new List<DeviceTransporterVm> { new () { Identifier = 1 }, new () { Identifier = 2 } };
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        Assert.ThrowsAsync<NullReferenceException>(async () => await _deviceReader.GetDevicesAsync(devices, cancellationToken));
    }

    [Test]
    public void GetDevicesAsync_WithEmptyDeviceList_ThrowsNullReferenceException()
    {
        // Arrange
        var devices = new List<DeviceTransporterVm>();
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        Assert.ThrowsAsync<NullReferenceException>(async () => await _deviceReader.GetDevicesAsync(devices, cancellationToken));
    }

    [Test]
    public void GetDevicesAsync_WithNullGeotabApi_ThrowsNullReferenceException_NoParams()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        Assert.ThrowsAsync<NullReferenceException>(async () => await _deviceReader.GetDevicesAsync(cancellationToken));
    }
}
