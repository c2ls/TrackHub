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

namespace TrackHub.Router.Infrastructure.Tracksolid.Tests;

[TestFixture]
public class TracksolidDeviceReaderTests
{
    private const string DeviceList = """
        {"code":0,"message":"success","result":[
          {"imei":"868120145233604","deviceName":"Truck 1","mcType":"GT300L","vehicleNumber":"ABC123","enabledFlag":1},
          {"imei":"868120145233605","deviceName":null,"mcType":"VL802","enabledFlag":1}
        ]}
        """;

    private TracksolidFakeHttpClientService _http = null!;
    private DeviceReader _reader = null!;

    [SetUp]
    public async Task SetUp()
    {
        _http = new TracksolidFakeHttpClientService(form => TracksolidTestData.Respond(form, DeviceList));
        _reader = new DeviceReader(TracksolidTestData.ClientFactory(), _http, new Mock<ICredentialWriter>().Object, new ProviderSessionStore());
        await _reader.Init(TracksolidTestData.Credential());
    }

    [Test]
    public async Task GetDevicesAsync_All_MapsImeiAsSerial()
    {
        var result = (await _reader.GetDevicesAsync(CancellationToken.None)).ToList();

        var request = _http.Requests.Last().Form;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(request["method"], Is.EqualTo("jimi.user.device.list"));
            Assert.That(request["target"], Is.EqualTo("fleet-account"));
            Assert.That(result.Select(d => d.Serial), Is.EqualTo(new[] { "868120145233604", "868120145233605" }));
            Assert.That(result.Select(d => d.Name), Is.EqualTo(new[] { "Truck 1", "868120145233605" }));
            Assert.That(result.All(d => d.Identifier == 0 && d.DeviceId == null), Is.True);
        }
    }

    [Test]
    public async Task GetDevicesAsync_Requested_ReturnsOnlyMatchingSerials()
    {
        var requested = new DeviceTransporterVm(Guid.NewGuid(), 7, "868120145233605", "Mine", "Truck", (short)TransporterType.Truck);

        var result = (await _reader.GetDevicesAsync([requested, requested with { Serial = "missing" }], CancellationToken.None)).ToList();

        Assert.That(result.Single().Serial, Is.EqualTo("868120145233605"));
        Assert.That(result.Single().Identifier, Is.EqualTo(7));
    }

    [Test]
    public async Task GetDeviceAsync_Unknown_Throws()
    {
        var requested = new DeviceTransporterVm(Guid.NewGuid(), 0, "missing", "Mine", "Truck", (short)TransporterType.Truck);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _reader.GetDeviceAsync(requested, CancellationToken.None));
    }

    [Test]
    public async Task GetDevicesAsync_ErrorPayload_Throws()
    {
        var http = new TracksolidFakeHttpClientService(form => TracksolidTestData.Respond(form, """{"code":1002,"message":"Account queried doesn't exist"}"""));
        var reader = new DeviceReader(TracksolidTestData.ClientFactory(), http, new Mock<ICredentialWriter>().Object, new ProviderSessionStore());
        await reader.Init(TracksolidTestData.Credential());

        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.GetDevicesAsync(CancellationToken.None));
    }
}
