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
    public void GetDevicePositionAsync_WithNullResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice", "IMEI123");
        var response = new TrackResponse(0, null);

        HttpClientServiceMock.Setup(x => x.GetAsync<TrackResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(response);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await PositionReader.GetDevicePositionAsync(deviceDto, TestCancellationToken));
    }

    [Test]
    public async Task GetDevicePositionAsync_WithValidResponse_ReturnsPositionVm()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice", "IMEI123");
        var trackRecord = new TrackRecord("IMEI123", 1419906952, 1419906052, 1419906952, 1419905754, 113.909813, 22.583197, 195, 60, 1400, 1, 0, 0, 1, 0, 2, 100);
        var response = new TrackResponse(0, [trackRecord]);

        HttpClientServiceMock.Setup(x => x.GetAsync<TrackResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await PositionReader.GetDevicePositionAsync(deviceDto, TestCancellationToken);

        // Assert
        Assert.That(result.TransporterId, Is.EqualTo(deviceDto.TransporterId));
        Assert.That(result.Latitude, Is.EqualTo(22.583197));
        Assert.That(result.Longitude, Is.EqualTo(113.909813));
        Assert.That(result.Speed, Is.EqualTo(60));
    }

    [Test]
    public async Task GetDevicePositionAsync_WithMultipleDevices_ReturnsEmptyList()
    {
        // Arrange
        var devices = new List<DeviceTransporterVm>
        {
            CreateDeviceTransporterVm(1, "TestDevice1", "IMEI123"),
            CreateDeviceTransporterVm(2, "TestDevice2", "IMEI456")
        };
        var response = new TrackResponse(0, null);

        HttpClientServiceMock.Setup(x => x.GetAsync<TrackResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }

    [Test]
    public async Task GetDevicePositionAsync_WithMultipleDevices_ReturnsPositions()
    {
        // Arrange
        var devices = new List<DeviceTransporterVm>
        {
            CreateDeviceTransporterVm(1, "TestDevice1", "IMEI123"),
            CreateDeviceTransporterVm(2, "TestDevice2", "IMEI456")
        };
        var track1 = new TrackRecord("IMEI123", 1419906952, 1419906052, 1419906952, 1419905754, 113.909813, 22.583197, 195, 60, 1400, 1, 0, 0, 1, 0, 2, 100);
        var track2 = new TrackRecord("IMEI456", 1419906952, 1419906052, 1419906952, 1419905754, 114.0, 23.0, 90, 80, 500, 0, -1, -1, 1, 1, 2, 80);
        var response = new TrackResponse(0, [track1, track2]);

        HttpClientServiceMock.Setup(x => x.GetAsync<TrackResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken);

        // Assert
        AssertIsNotEmpty(result);
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetPositionAsync_WithEmptyPlayback_ReturnsEmptyList()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice", "IMEI123");
        var from = DateTimeOffset.Now.AddHours(-1);
        var to = DateTimeOffset.Now;
        var response = new PlaybackResponse(0, null);

        HttpClientServiceMock.Setup(x => x.GetAsync<PlaybackResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await PositionReader.GetPositionAsync(from, to, deviceDto, TestCancellationToken);

        // Assert
        AssertIsEmpty(result);
    }

    [Test]
    public async Task GetPositionAsync_WithPlaybackData_ReturnsPositions()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice", "IMEI123");
        var from = DateTimeOffset.Now.AddHours(-1);
        var to = DateTimeOffset.Now;
        var playbackRecord = "113.97196,22.568616,1406858664,0,228;113.97196,22.56861,1406858684,0,228;113.97196,22.56861,1406858704,0,228";
        var response = new PlaybackResponse(0, playbackRecord);

        HttpClientServiceMock.Setup(x => x.GetAsync<PlaybackResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await PositionReader.GetPositionAsync(from, to, deviceDto, TestCancellationToken);

        // Assert
        AssertIsNotEmpty(result);
        Assert.That(result.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task GetPositionAsync_WithPagedPlayback_AggregatesAllPages()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice", "IMEI123");
        var from = DateTimeOffset.UtcNow.AddHours(-2);
        var to = DateTimeOffset.UtcNow;

        // Build a first page with exactly 1000 records
        var entries = new List<string>();
        var baseTime = from.ToUnixTimeSeconds();
        for (int i = 0; i < 1000; i++)
        {
            entries.Add($"113.0,22.0,{baseTime + i},10,90");
        }
        var firstPageRecord = string.Join(";", entries);
        var firstPageResponse = new PlaybackResponse(0, firstPageRecord);

        // Second page with fewer than 1000 records (end of data)
        var secondPageRecord = $"114.0,23.0,{baseTime + 1000},20,180;115.0,24.0,{baseTime + 1001},30,270";
        var secondPageResponse = new PlaybackResponse(0, secondPageRecord);

        var callCount = 0;
        HttpClientServiceMock.Setup(x => x.GetAsync<PlaybackResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), TestCancellationToken))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1 ? firstPageResponse : secondPageResponse;
            });

        // Act
        var result = await PositionReader.GetPositionAsync(from, to, deviceDto, TestCancellationToken);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(1002));
        Assert.That(callCount, Is.EqualTo(2));
    }

    [Test]
    public void ParsePlaybackRecord_WithValidData_ReturnsPositions()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice", "IMEI123");
        var record = "113.97196,22.568616,1406858664,0,228;113.97196,22.56861,1406858684,0,228";

        // Act
        var result = PositionReader.ParsePlaybackRecord(record, deviceDto);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Longitude, Is.EqualTo(113.97196));
        Assert.That(result[0].Latitude, Is.EqualTo(22.568616));
        Assert.That(result[0].Speed, Is.EqualTo(0));
        Assert.That(result[0].Course, Is.EqualTo(228));
    }

    [Test]
    public void ParsePlaybackRecord_WithEmptyString_ReturnsEmptyList()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice", "IMEI123");

        // Act
        var result = PositionReader.ParsePlaybackRecord("", deviceDto);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParsePlaybackRecord_WithInvalidData_SkipsInvalidEntries()
    {
        // Arrange
        var deviceDto = CreateDeviceTransporterVm(1, "TestDevice", "IMEI123");
        var record = "113.97196,22.568616,1406858664,0,228;invalid,data;113.97196,22.56861,1406858684,0,228";

        // Act
        var result = PositionReader.ParsePlaybackRecord(record, deviceDto);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
    }
}
