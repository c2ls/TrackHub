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

using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Geotab.Tests;

[TestFixture]
public class PositionReaderTests : PositionReaderTestsBase<PositionReader>
{
    protected override PositionReader CreatePositionReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
        => new(SessionStoreMock.Object);

    [Test]
    public void GetDevicePositionAsync_WithNullGeotabApi_ThrowsNullReferenceException()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice");

        // Act & Assert
        Assert.ThrowsAsync<NullReferenceException>(
            async () => await PositionReader.GetDevicePositionAsync(deviceDto, TestCancellationToken));
    }

    [Test]
    public void GetDevicePositionAsync_WithMultipleDevices_ThrowsNullReferenceException()
    {
        // Arrange
        var devices = CreateDeviceTransporterVmList(1, 2);

        // Act & Assert
        Assert.ThrowsAsync<NullReferenceException>(
            async () => await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken));
    }

    [Test]
    public void GetPositionAsync_WithDateRange_ThrowsNullReferenceException()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice");
        var from = DateTimeOffset.Now.AddHours(-1);
        var to = DateTimeOffset.Now;

        // Act & Assert
        Assert.ThrowsAsync<NullReferenceException>(
            async () => await PositionReader.GetPositionAsync(from, to, deviceDto, TestCancellationToken));
    }
}
