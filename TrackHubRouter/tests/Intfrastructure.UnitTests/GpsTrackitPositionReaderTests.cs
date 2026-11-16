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
using TrackHub.Router.Infrastructure.GpsTrackit.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.GpsTrackit.Tests;

[TestFixture]
public class PositionReaderTests : PositionReaderTestsBase<PositionReader>
{
    private readonly CredentialTokenDto _credential = new(Guid.NewGuid(), "https://cloud-api.gpstrackit.com/", "", "", "api-key", null, null, null, null, null);
    private HttpClient _httpClient = null!;
    private TimeSpan _interval;

    protected override PositionReader CreatePositionReader(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService);

    [SetUp]
    public async Task SetUp()
    {
        _interval = GpsTrackitReaderBase.MinRequestInterval;
        GpsTrackitReaderBase.MinRequestInterval = TimeSpan.Zero;
        _httpClient = new HttpClient();
        HttpClientFactoryMock.Setup(x => x.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(_httpClient);
        await PositionReader.Init(_credential, TestCancellationToken);
    }

    [TearDown]
    public void TearDown()
    {
        GpsTrackitReaderBase.MinRequestInterval = _interval;
        _httpClient.Dispose();
    }

    private static Unit Located(long id) => new(id, $"Unit {id}", null, new LastReading(1738802189, 1, 2, 0, 0, null, null, null));

    private static string Body(object? body) => JsonSerializer.Serialize(body, (JsonSerializerOptions?)null);

    [Test]
    public void Init_SendsTheApiKeyHeader()
        => Assert.That(_httpClient.DefaultRequestHeaders.GetValues("x-api-key"), Is.EqualTo(new[] { "api-key" }));

    [Test]
    public async Task GetDevicePositionAsync_PagesThroughUnitsByOffset_AndKeepsRequestedUnits()
    {
        var firstPage = Enumerable.Range(1, GpsTrackitReaderBase.UnitPageSize).Select(i => Located(i)).ToList();
        HttpClientServiceMock.Setup(x => x.PostAsync<SearchPage<Unit>>("unit/search", It.Is<object?>(b => Body(b).Contains("\"offset\":0")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchPage<Unit>(firstPage, 101));
        HttpClientServiceMock.Setup(x => x.PostAsync<SearchPage<Unit>>("unit/search", It.Is<object?>(b => Body(b).Contains("\"offset\":100")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchPage<Unit>([Located(101)], 101));
        var devices = new[] { CreateDeviceTransporterVm(serial: "7"), CreateDeviceTransporterVm(serial: "101") };

        var positions = (await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken)).ToList();

        Assert.That(positions.Select(p => p.TransporterId), Is.EquivalentTo(devices.Select(d => d.TransporterId)));
    }

    [Test]
    public async Task GetPositionAsync_NoReadings_IsAnEmptyHistory()
    {
        HttpClientServiceMock.Setup(x => x.PostAsync<SearchPage<Reading>>("reading/search", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("not found", null, HttpStatusCode.NotFound));
        var from = new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

        var positions = await PositionReader.GetPositionAsync(from, from.AddHours(1), CreateDeviceTransporterVm(serial: "7"), TestCancellationToken);

        Assert.That(positions, Is.Empty);
    }

    [Test]
    public async Task GetPositionAsync_SendsTheIsoRangeForTheUnit()
    {
        var from = new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);
        HttpClientServiceMock.Setup(x => x.PostAsync<SearchPage<Reading>>("reading/search", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchPage<Reading>([new Reading(from.AddMinutes(1), 1, 2, 0, 0, null, false, null, null, null)], 1));

        var positions = (await PositionReader.GetPositionAsync(from, from.AddHours(1), CreateDeviceTransporterVm(serial: "7"), TestCancellationToken)).ToList();

        Assert.That(positions, Has.Count.EqualTo(1));
        HttpClientServiceMock.Verify(x => x.PostAsync<SearchPage<Reading>>("reading/search",
            It.Is<object?>(b => Body(b) == "{\"deviceId\":\"7\",\"dateFrom\":\"2026-03-01T10:00:00.000Z\",\"dateTo\":\"2026-03-01T11:00:00.000Z\",\"limit\":500,\"offset\":0}"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetDevicePositionAsync_InvalidKey_Throws()
    {
        HttpClientServiceMock.Setup(x => x.PostAsync<SearchPage<Unit>>("unit/search", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("unauthorized", null, HttpStatusCode.Unauthorized));

        await Assert.ThrowsAsync<HttpRequestException>(() => PositionReader.GetDevicePositionAsync([CreateDeviceTransporterVm(serial: "7")], TestCancellationToken));
    }

    [Test]
    public async Task Ping_NotFound_IsAFailure_NotAnEmptyAccount()
    {
        var tester = new ConnectivityTester(HttpClientFactoryMock.Object, HttpClientServiceMock.Object);
        HttpClientServiceMock.Setup(x => x.PostAsync<SearchPage<Unit>>("unit/search", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("not found", null, HttpStatusCode.NotFound));

        await Assert.ThrowsAsync<HttpRequestException>(() => tester.Ping(_credential, TestCancellationToken));
    }

    [Test]
    public async Task GetDevicePositionAsync_ShortPageBelowTheTotal_KeepsPaging()
    {
        HttpClientServiceMock.Setup(x => x.PostAsync<SearchPage<Unit>>("unit/search", It.Is<object?>(b => Body(b).Contains("\"offset\":0")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchPage<Unit>([Located(1)], 2));
        HttpClientServiceMock.Setup(x => x.PostAsync<SearchPage<Unit>>("unit/search", It.Is<object?>(b => Body(b).Contains("\"offset\":1")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchPage<Unit>([Located(2)], 2));
        var devices = new[] { CreateDeviceTransporterVm(serial: "1"), CreateDeviceTransporterVm(serial: "2") };

        var positions = (await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken)).ToList();

        Assert.That(positions, Has.Count.EqualTo(2));
    }
}
