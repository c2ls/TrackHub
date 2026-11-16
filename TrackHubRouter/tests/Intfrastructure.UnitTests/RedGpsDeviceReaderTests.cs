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
using TrackHub.Router.Infrastructure.Common;
using TrackHub.Router.Infrastructure.RedGps.Helpers;

namespace TrackHub.Router.Infrastructure.RedGps.Tests;

[TestFixture]
public class RedGpsDeviceReaderTests
{
    private const string Vehicles = """
        {"status":200,"data":[
          {"id":"198098","nombre":"Jeep Patriot","patente":"NT-53-53","idgps":"0580012342332337","odometro":"25682.2"},
          {"id":198099,"nombre":null,"patente":"XX-1","idgps":"0580012342332338"},
          {"id":"198100","nombre":"No GPS","patente":"YY-2","idgps":""}
        ]}
        """;

    private RedGpsFakeHttpClientService _http = null!;
    private DeviceReader _reader = null!;

    [SetUp]
    public async Task SetUp()
    {
        RequestPacer.MinInterval = TimeSpan.Zero;
        _http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, Vehicles));
        _reader = new DeviceReader(RedGpsTestData.ClientFactory(), _http, new Mock<ICredentialWriter>().Object, new ProviderSessionStore());
        await _reader.Init(RedGpsTestData.Credential());
    }

    [TearDown]
    public void TearDown() => RequestPacer.MinInterval = TimeSpan.FromSeconds(30);

    [Test]
    public async Task GetDevicesAsync_All_MapsIdgpsAsSerialAndIdAsIdentifier()
    {
        var result = (await _reader.GetDevicesAsync(CancellationToken.None)).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_http.Requests.Last().Action, Is.EqualTo("vehicleGetAll"));
            Assert.That(result.Select(d => d.Serial), Is.EqualTo(new[] { "0580012342332337", "0580012342332338" }));
            Assert.That(result.Select(d => d.Identifier), Is.EqualTo(new[] { 198098, 198099 }));
            Assert.That(result.Select(d => d.Name), Is.EqualTo(new[] { "Jeep Patriot", "0580012342332338" }));
            Assert.That(result.All(d => d.DeviceId == null), Is.True);
        }
    }

    [Test]
    public async Task GetDevicesAsync_Requested_ReturnsOnlyMatchingSerials()
    {
        var requested = new DeviceTransporterVm(Guid.NewGuid(), 0, "0580012342332338", "Mine", "Truck", (short)TransporterType.Truck);

        var result = (await _reader.GetDevicesAsync([requested, requested with { Serial = "missing" }], CancellationToken.None)).ToList();

        Assert.That(result.Single().Serial, Is.EqualTo("0580012342332338"));
        Assert.That(result.Single().DeviceId, Is.EqualTo(requested.TransporterId));
    }

    [Test]
    public async Task GetDeviceAsync_Unknown_Throws()
    {
        var requested = new DeviceTransporterVm(Guid.NewGuid(), 0, "missing", "Mine", "Truck", (short)TransporterType.Truck);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _reader.GetDeviceAsync(requested, CancellationToken.None));
    }

    [Test]
    public async Task GetDevicesAsync_ErrorStatus_Throws()
    {
        var http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, """{"status":60500,"data":""}"""));
        var reader = new DeviceReader(RedGpsTestData.ClientFactory(), http, new Mock<ICredentialWriter>().Object, new ProviderSessionStore());
        await reader.Init(RedGpsTestData.Credential());

        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.GetDevicesAsync(CancellationToken.None));
    }
}
