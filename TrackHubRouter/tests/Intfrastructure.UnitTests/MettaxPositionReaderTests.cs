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

using System.Net;
using System.Text.Json;
using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.Mettax.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Mettax.Tests;

[TestFixture]
public class PositionReaderTests : PositionReaderTestsBase<PositionReader>
{
    private const string TokenUrl = "v2/openapi/system/createToken";
    private readonly CredentialTokenDto _credential = new(Guid.NewGuid(), "https://mettahub.mettaxiot.com/gps", "", "", "api-key", "api-secret", null, null, null, null);
    private HttpClient _httpClient = null!;

    protected override PositionReader CreatePositionReader(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService, SessionStoreMock.Object);

    [SetUp]
    public async Task SetUp()
    {
        _httpClient = new HttpClient();
        HttpClientFactoryMock.Setup(x => x.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(_httpClient);
        HttpClientServiceMock.SetupSequence(x => x.PostAsync<MettaxResponse<string>>(TokenUrl, It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MettaxResponse<string>(0, "success", "token-1"))
            .ReturnsAsync(new MettaxResponse<string>(0, "success", "token-2"));
        await PositionReader.Init(_credential, TestCancellationToken);
    }

    [TearDown]
    public void TearDown() => _httpClient.Dispose();

    private static DeviceShadow Shadow(string id, double lat, double lon = -74)
        => new(new Fix(id, null, "2026-03-01 10:15:30", lat, lon, 30, 90, true, 8, null, 1, null, null), new ShadowExpand("2026-03-01 10:15:00"));

    [Test]
    public void Init_CreatesTheTokenFromKeyAndSecret_SendsItRaw_AndCachesIt()
    {
        using (Assert.EnterMultipleScope())
        {
            HttpClientServiceMock.Verify(x => x.PostAsync<MettaxResponse<string>>(TokenUrl,
                It.Is<object?>(body => JsonSerializer.Serialize(body, (JsonSerializerOptions?)null) == "{\"apiKey\":\"api-key\",\"apiSecret\":\"api-secret\"}"),
                It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(_httpClient.DefaultRequestHeaders.GetValues("Authorization"), Is.EqualTo(new[] { "token-1" }));
            SessionStoreMock.Verify(x => x.Set(_credential, "token-1", It.Is<TimeSpan>(ttl => ttl > TimeSpan.Zero), true), Times.Once);
        }
    }

    [Test]
    public async Task Init_FailedAuthentication_Throws()
    {
        HttpClientServiceMock.Setup(x => x.PostAsync<MettaxResponse<string>>(TokenUrl, It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MettaxResponse<string>(400, "Incorrect request parameter", null));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => PositionReader.Init(_credential, TestCancellationToken));
        Assert.That(exception!.Message, Does.Contain("Incorrect request parameter"));
    }

    [Test]
    public async Task GetDevicePositionAsync_ManyDevices_ReadsTheShadowOnce_AndKeepsOnlyRequestedDevicesWithAFix()
    {
        HttpClientServiceMock.Setup(x => x.PostAsync<MettaxResponse<IReadOnlyList<DeviceShadow>>>(PositionReader.ShadowUrl, It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MettaxResponse<IReadOnlyList<DeviceShadow>>(0, null, [Shadow("a", 1), Shadow("b", 0, 0), Shadow("other", 2)]));
        var devices = new[] { CreateDeviceTransporterVm(serial: "a"), CreateDeviceTransporterVm(serial: "b") };

        var positions = (await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken)).ToList();

        Assert.That(positions.Single().TransporterId, Is.EqualTo(devices[0].TransporterId));
        Assert.That(positions.Single().DeviceDateTime, Is.EqualTo(new DateTimeOffset(2026, 3, 1, 10, 15, 0, TimeSpan.Zero)));
        HttpClientServiceMock.Verify(x => x.PostAsync<MettaxResponse<IReadOnlyList<DeviceShadow>>>(PositionReader.ShadowUrl,
            It.Is<object?>(body => JsonSerializer.Serialize(body, (JsonSerializerOptions?)null) == "{\"deviceIds\":\"a,b\"}"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetDevicePositionAsync_ErrorCode_Throws()
    {
        HttpClientServiceMock.Setup(x => x.PostAsync<MettaxResponse<IReadOnlyList<DeviceShadow>>>(PositionReader.ShadowUrl, It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MettaxResponse<IReadOnlyList<DeviceShadow>>(63001, "The current call count exceeds the limit", null));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => PositionReader.GetDevicePositionAsync([CreateDeviceTransporterVm(serial: "a")], TestCancellationToken));
        Assert.That(exception!.Message, Does.Contain("63001"));
    }

    [Test]
    public async Task GetDevicePositionAsync_RejectedToken_RenewsItOnce_AndRetries()
    {
        HttpClientServiceMock.SetupSequence(x => x.PostAsync<MettaxResponse<IReadOnlyList<DeviceShadow>>>(PositionReader.ShadowUrl, It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("expired", null, HttpStatusCode.Unauthorized))
            .ReturnsAsync(new MettaxResponse<IReadOnlyList<DeviceShadow>>(0, null, [Shadow("a", 1)]));

        var positions = (await PositionReader.GetDevicePositionAsync([CreateDeviceTransporterVm(serial: "a")], TestCancellationToken)).ToList();

        Assert.That(positions, Has.Count.EqualTo(1));
        Assert.That(_httpClient.DefaultRequestHeaders.GetValues("Authorization"), Is.EqualTo(new[] { "token-2" }));
        SessionStoreMock.Verify(x => x.Invalidate(_credential.CredentialId), Times.Once);
    }

    [Test]
    public async Task GetPositionAsync_PagesUntilAShortPage_AndKeepsTheRequestedRange()
    {
        var full = Enumerable.Range(0, PositionReader.HistoryPageSize)
            .Select(i => new Fix("a", $"2026-03-01 10:{i % 60:00}:00", null, 1, 2, 10, 0, false, 5, null, 1, null, null))
            .ToList();
        HttpClientServiceMock.SetupSequence(x => x.PostAsync<MettaxResponse<Page<Fix>>>(PositionReader.HistoryUrl, It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MettaxResponse<Page<Fix>>(0, null, new Page<Fix>(full, 101)))
            .ReturnsAsync(new MettaxResponse<Page<Fix>>(0, null, new Page<Fix>([new Fix("a", "2026-03-02 00:00:00", null, 1, 2, 10, 0, false, 5, null, 1, null, null)], 101)));
        var from = new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

        var positions = (await PositionReader.GetPositionAsync(from, from.AddHours(1), CreateDeviceTransporterVm(serial: "a"), TestCancellationToken)).ToList();

        Assert.That(positions, Has.Count.EqualTo(60));
        HttpClientServiceMock.Verify(x => x.PostAsync<MettaxResponse<Page<Fix>>>(PositionReader.HistoryUrl,
            It.Is<object?>(body => JsonSerializer.Serialize(body, (JsonSerializerOptions?)null).Contains("\"startTime\":\"2026-03-01 10:00:00\"")),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task GetPositionAsync_ServerCapsThePageSize_KeepsPagingUntilTheTotal()
    {
        static Fix Record(int minute) => new("a", $"2026-03-01 10:{minute:00}:00", null, 1, 2, 10, 0, false, 5, null, 1, null, null);
        HttpClientServiceMock.SetupSequence(x => x.PostAsync<MettaxResponse<Page<Fix>>>(PositionReader.HistoryUrl, It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MettaxResponse<Page<Fix>>(0, null, new Page<Fix>([Record(1), Record(2)], 3)))
            .ReturnsAsync(new MettaxResponse<Page<Fix>>(0, null, new Page<Fix>([Record(3)], 3)));
        var from = new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

        var positions = (await PositionReader.GetPositionAsync(from, from.AddHours(1), CreateDeviceTransporterVm(serial: "a"), TestCancellationToken)).ToList();

        Assert.That(positions, Has.Count.EqualTo(3));
    }
}
