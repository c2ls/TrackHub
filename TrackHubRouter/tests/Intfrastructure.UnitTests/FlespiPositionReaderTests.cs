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

using TrackHub.Router.Infrastructure.Flespi.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Flespi.Tests;

[TestFixture]
public class PositionReaderTests : PositionReaderTestsBase<PositionReader>
{
    protected override PositionReader CreatePositionReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService);

    [Test]
    public void GetDevicePositionAsync_WithNullResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice");
        var response = new MessageListResponse(null);

        HttpClientServiceMock.Setup(x => x.GetAsync<MessageListResponse>(
            It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(response);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await PositionReader.GetDevicePositionAsync(deviceDto, TestCancellationToken));
    }

    [Test]
    public async Task GetDevicePositionAsync_WithMultipleDevices_ReturnsPositionVmList()
    {
        // Arrange
        var devices = CreateDeviceTransporterVmList(1, 2);
        var response = new MessageListResponse(null);

        HttpClientServiceMock.Setup(x => x.GetAsync<MessageListResponse>(
            It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(response);

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
        var response = new MessageListResponse(new List<Message>());

        HttpClientServiceMock.Setup(x => x.GetAsync<MessageListResponse>(
            It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(response);

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
        var response = new MessageListResponse(null);

        HttpClientServiceMock.Setup(x => x.GetAsync<MessageListResponse>(
            It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await PositionReader.GetPositionAsync(from, to, deviceDto, TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }
}
