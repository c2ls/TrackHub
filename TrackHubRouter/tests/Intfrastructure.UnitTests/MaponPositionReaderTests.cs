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

using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.Mapon.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Mapon.Tests;

[TestFixture]
public class PositionReaderTests : PositionReaderTestsBase<PositionReader>
{
    private readonly CredentialTokenDto _credential = new(Guid.NewGuid(), "https://mapon.com/api/v1/", "", "", "key-1", null, null, null, null, null);
    private HttpClient _httpClient = null!;

    protected override PositionReader CreatePositionReader(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService);

    [SetUp]
    public async Task SetUp()
    {
        _httpClient = new HttpClient();
        HttpClientFactoryMock.Setup(x => x.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(_httpClient);
        await PositionReader.Init(_credential, TestCancellationToken);
    }

    [TearDown]
    public void TearDown() => _httpClient.Dispose();

    private static Unit Located(long id)
        => new(id, $"P{id}", null, 1, 2, new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero), 10, 90, null, null, null);

    [Test]
    public async Task GetDevicePositionAsync_ManyDevices_ReadsTheUnitListOnceWithIgnitionAndAltitude()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<MaponResponse<UnitList>>(
                "unit/list.json?key=key-1&include%5B%5D=ignition&include%5B%5D=altitude", It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MaponResponse<UnitList>(new UnitList([Located(1), Located(2), Located(3)]), null));
        var devices = new[] { CreateDeviceTransporterVm(serial: "1"), CreateDeviceTransporterVm(serial: "3") };

        var positions = (await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken)).ToList();

        Assert.That(positions.Select(p => p.TransporterId), Is.EquivalentTo(devices.Select(d => d.TransporterId)));
    }

    [Test]
    public async Task GetDevicePositionAsync_ErrorPayload_Throws()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<MaponResponse<UnitList>>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MaponResponse<UnitList>(null, new MaponError(1011, "Request limit reached")));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => PositionReader.GetDevicePositionAsync([CreateDeviceTransporterVm(serial: "1")], TestCancellationToken));
        Assert.That(exception!.Message, Does.Contain("Request limit reached"));
    }

    [Test]
    public async Task GetPositionAsync_ReadsOnlyRoutePointsInTheRequestedRange()
    {
        var from = new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);
        HttpClientServiceMock.Setup(x => x.GetAsync<MaponResponse<RouteList>>(
                "route/list.json?key=key-1&unit_id=7&from=2026-03-01T10%3A00%3A00Z&till=2026-03-01T11%3A00%3A00Z&include%5B%5D=decoded_route",
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MaponResponse<RouteList>(new RouteList([new UnitRoutes(
            [
                new Route("route", new DecodedRoute([new RoutePoint(from.AddMinutes(10), 1, 2, 30), new RoutePoint(from.AddMinutes(-1), 1, 2, 30)])),
                new Route("stop", new DecodedRoute([new RoutePoint(from.AddMinutes(20), 1, 2, 0)]))
            ])]), null));

        var positions = (await PositionReader.GetPositionAsync(from, from.AddHours(1), CreateDeviceTransporterVm(serial: "7"), TestCancellationToken)).ToList();

        Assert.That(positions.Single().DeviceDateTime, Is.EqualTo(from.AddMinutes(10)));
    }
}
