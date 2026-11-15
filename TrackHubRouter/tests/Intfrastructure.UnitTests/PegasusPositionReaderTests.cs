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
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.Pegasus.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Pegasus.Tests;

[TestFixture]
public class PositionReaderTests : PositionReaderTestsBase<PositionReader>
{
    private readonly CredentialTokenDto _credential = new(Guid.NewGuid(), "https://site.example/api", "user", "secret", null, null, null, null, null, null);
    private Mock<ICredentialWriter> _credentialWriterMock = null!;
    private readonly List<string> _requestedUrls = [];

    protected override PositionReader CreatePositionReader(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
    {
        _credentialWriterMock = new Mock<ICredentialWriter>();
        return new PositionReader(httpClientFactory, httpClientService, _credentialWriterMock.Object, SessionStoreMock.Object);
    }

    [SetUp]
    public async Task SetUp()
    {
        _requestedUrls.Clear();
        HttpClientFactoryMock.Setup(x => x.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(() => new HttpClient());
        HttpClientServiceMock.Setup(x => x.PostAsync<LoginResponse>("login", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResponse("User successfully authenticated", "token-1"));
        await PositionReader.Init(_credential, TestCancellationToken);
    }

    private void SetupGet<T>(Func<string, T?> respond)
        => HttpClientServiceMock.Setup(x => x.GetAsync<T>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string url, IDictionary<string, string>? _, CancellationToken _) =>
            {
                _requestedUrls.Add(url);
                return respond(url);
            });

    private static VehicleLatest Latest(long id, long imei, double lat)
        => new(id, $"Vehicle {id}", new LatestDevice(imei, new DeviceLatest(new LatestLocation(lat, -74, 90, 30, 1700000000, 1700000002), null, null)));

    private static RawEvent Event(DateTimeOffset at)
        => new(at.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss"), null, 4.6, -74.1, null, 10, 90, 8, true, null, null, null);

    [Test]
    public async Task GetDevicePositionAsync_ReadsTheVehicleByIdentifierWithTheLatestSelect()
    {
        SetupGet(_ => Latest(42, 867162029806270, 4.5));

        var position = await PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(42, serial: "867162029806270"), TestCancellationToken);

        Assert.That(position.Latitude, Is.EqualTo(4.5));
        Assert.That(_requestedUrls.Single(), Is.EqualTo("vehicles/42?select=device:latest.loc+latest.ios+latest.counters"));
    }

    [Test]
    public async Task GetDevicePositionAsync_VehicleWithoutLocation_Throws()
    {
        SetupGet(_ => new VehicleLatest(42, "Vehicle", new LatestDevice(867162029806270, new DeviceLatest(null, null, null))));

        await Assert.ThrowsAsync<InvalidOperationException>(() => PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(42), TestCancellationToken));
    }

    [Test]
    public async Task GetDevicePositionAsync_ManyDevices_ResponseWithoutAVehicleList_Throws()
    {
        SetupGet(_ => new PagedResponse<VehicleLatest>(null, 0, 0));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => PositionReader.GetDevicePositionAsync([CreateDeviceTransporterVm(1, serial: "111")], TestCancellationToken));
    }

    [Test]
    public async Task GetDevicePositionAsync_ManyDevices_SkipsInvalidFixes()
    {
        var invalid = new VehicleLatest(1, "Vehicle 1", new LatestDevice(111,
            new DeviceLatest(new LatestLocation(1, -74, 90, 30, 1700000000, 1700000002, Valid: false), null, null)));
        SetupGet(_ => new PagedResponse<VehicleLatest>([invalid, Latest(2, 222, 2)], 1, 1));
        var devices = new[] { CreateDeviceTransporterVm(1, serial: "111"), CreateDeviceTransporterVm(2, serial: "222") };

        var positions = await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken);

        Assert.That(positions.Select(p => p.Latitude), Is.EqualTo(new[] { 2d }));
    }

    [Test]
    public async Task GetDevicePositionAsync_ManyDevices_PagesThroughEveryVehicle_AndReturnsOnlyRequestedDevices()
    {
        SetupGet(url => url.Contains("page=1")
            ? new PagedResponse<VehicleLatest>([Latest(1, 111, 1), Latest(2, 222, 2)], 1, 2)
            : new PagedResponse<VehicleLatest>([Latest(3, 333, 3)], 2, 2));
        var devices = new[] { CreateDeviceTransporterVm(1, serial: "111"), CreateDeviceTransporterVm(3, serial: "333") };

        var positions = (await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken)).ToList();

        Assert.That(positions.Select(p => p.TransporterId), Is.EquivalentTo(devices.Select(d => d.TransporterId)));
        Assert.That(_requestedUrls, Is.EqualTo(new[]
        {
            "vehicles?set=1000&page=1&select=device:latest.loc+latest.ios+latest.counters",
            "vehicles?set=1000&page=2&select=device:latest.loc+latest.ios+latest.counters"
        }));
    }

    [Test]
    public async Task GetPositionAsync_RequestsValidPositionsInUtc_AndDropsPointsOutsideTheRange()
    {
        var from = new DateTimeOffset(2026, 3, 1, 5, 0, 0, TimeSpan.FromHours(-5));
        var to = from.AddHours(1);
        SetupGet(_ => new RawdataResponse([Event(from.AddMinutes(-1)), Event(from), Event(from.AddMinutes(30)), Event(to.AddSeconds(1))]));

        var positions = (await PositionReader.GetPositionAsync(from, to, CreateDeviceTransporterVm(7), TestCancellationToken)).ToList();

        Assert.That(positions.Select(p => p.DeviceDateTime), Is.EqualTo(new[] { from, from.AddMinutes(30) }));
        Assert.That(_requestedUrls.Single(), Is.EqualTo(
            "rawdata?vehicles=7&from=2026-03-01T10:00:00&to=2026-03-01T11:00:00"
            + "&fields=lat,lon,al,mph,head,sv,io_ign,dev_dist,dev_ign,temp,valid_position&filter=valid_position"));
    }

    [Test]
    public async Task GetPositionAsync_WindowAtTheEventCap_IsHalvedAndReadAgain()
    {
        var from = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddHours(2);
        var capped = new RawdataResponse(Enumerable.Repeat(Event(from), PositionReader.MaxEventsPerRequest).ToList());
        SetupGet(url => _requestedUrls.Count == 1
            ? capped
            : new RawdataResponse([Event(url.Contains("from=2026-03-01T00:00:00") ? from.AddMinutes(10) : from.AddMinutes(90))]));

        var positions = (await PositionReader.GetPositionAsync(from, to, CreateDeviceTransporterVm(7), TestCancellationToken)).ToList();

        Assert.That(positions.Select(p => p.DeviceDateTime), Is.EqualTo(new[] { from.AddMinutes(10), from.AddMinutes(90) }));
        Assert.That(_requestedUrls, Has.Count.EqualTo(3));
        Assert.That(_requestedUrls[1], Does.Contain("from=2026-03-01T00:00:00&to=2026-03-01T01:00:00"));
        Assert.That(_requestedUrls[2], Does.Contain("from=2026-03-01T01:00:00&to=2026-03-01T02:00:00"));
    }

    [Test]
    public async Task Unauthorized_LogsInAgainAndRetriesOnceWithTheNewToken()
    {
        HttpClientServiceMock.Setup(x => x.PostAsync<LoginResponse>("login", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResponse(null, "token-2"));
        HttpClientServiceMock.Setup(x => x.GetAsync<VehicleLatest>(It.IsAny<string>(), It.Is<IDictionary<string, string>>(h => h["Authenticate"] == "token-1"), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("expired", null, HttpStatusCode.Unauthorized));
        HttpClientServiceMock.Setup(x => x.GetAsync<VehicleLatest>(It.IsAny<string>(), It.Is<IDictionary<string, string>>(h => h["Authenticate"] == "token-2"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Latest(42, 1, 4.5));

        var position = await PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(42), TestCancellationToken);

        Assert.That(position.Latitude, Is.EqualTo(4.5));
        SessionStoreMock.Verify(x => x.Invalidate(_credential.CredentialId), Times.Once);
        _credentialWriterMock.Verify(x => x.UpdateTokenAsync(_credential.CredentialId, It.Is<UpdateTokenDto>(t => t.Token == "token-2"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Unauthorized_AfterTheRetry_Throws()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<VehicleLatest>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("denied", null, HttpStatusCode.Unauthorized));

        await Assert.ThrowsAsync<HttpRequestException>(() => PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(42), TestCancellationToken));
        HttpClientServiceMock.Verify(x => x.GetAsync<VehicleLatest>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task TooManyRequests_IsNeverRetried()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<RawdataResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Rate exceeded", null, HttpStatusCode.TooManyRequests));

        await Assert.ThrowsAsync<HttpRequestException>(() => PositionReader.GetPositionAsync(DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, CreateDeviceTransporterVm(7), TestCancellationToken));
        HttpClientServiceMock.Verify(x => x.GetAsync<RawdataResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
        HttpClientServiceMock.Verify(x => x.PostAsync<LoginResponse>("login", It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
