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

using System.Text.Json;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.Pegasus.Helpers;
using TrackHub.Router.Infrastructure.Pegasus.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Pegasus.Tests;

[TestFixture]
public class DeviceReaderTests : DeviceReaderTestsBase<DeviceReader>
{
    private CredentialTokenDto _credential = new(Guid.NewGuid(), "https://site.example/api", "user", "secret", null, null, null, null, null, null);
    private Mock<ICredentialWriter> _credentialWriterMock = null!;

    protected override DeviceReader CreateDeviceReader(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
    {
        _credentialWriterMock = new Mock<ICredentialWriter>();
        return new DeviceReader(httpClientFactory, httpClientService, _credentialWriterMock.Object, SessionStoreMock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        HttpClientFactoryMock.Setup(x => x.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(() => new HttpClient());
        HttpClientServiceMock.Setup(x => x.PostAsync<LoginResponse>("login", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResponse("User successfully authenticated", "token-1"));
    }

    private void SetupVehiclePages(params PagedResponse<Vehicle>[] pages)
        => HttpClientServiceMock.Setup(x => x.GetAsync<PagedResponse<Vehicle>>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string url, IDictionary<string, string>? _, CancellationToken _) => pages.Single(p => url.Contains($"page={p.Page}")));

    [Test]
    public async Task Init_WithoutSession_LogsInWithUsernameAndPassword_PersistsAndCachesASlidingSession()
    {
        await DeviceReader.Init(_credential, TestCancellationToken);

        HttpClientServiceMock.Verify(x => x.PostAsync<LoginResponse>("login",
            It.Is<object?>(body => JsonSerializer.Serialize(body, (JsonSerializerOptions?)null) == "{\"username\":\"user\",\"password\":\"secret\"}"),
            It.IsAny<CancellationToken>()), Times.Once);
        _credentialWriterMock.Verify(x => x.UpdateTokenAsync(_credential.CredentialId,
            It.Is<UpdateTokenDto>(t => t.Token == "token-1" && t.TokenExpiration > DateTimeOffset.UtcNow), It.IsAny<CancellationToken>()), Times.Once);
        SessionStoreMock.Verify(x => x.Set(_credential, "token-1", SessionHelper.SessionTtl, true), Times.Once);
        Assert.That(SessionHelper.SessionTtl, Is.LessThan(TimeSpan.FromMinutes(60)));
    }

    [Test]
    public async Task Init_WithCachedSession_DoesNotLogIn()
    {
        var cached = "cached-token";
        SessionStoreMock.Setup(x => x.TryGet(It.IsAny<CredentialTokenDto>(), out cached)).Returns(true);
        SetupVehiclePages(new PagedResponse<Vehicle>([], 1, 1));

        await DeviceReader.Init(_credential, TestCancellationToken);
        await DeviceReader.GetDevicesAsync(TestCancellationToken);

        HttpClientServiceMock.Verify(x => x.PostAsync<LoginResponse>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Never);
        HttpClientServiceMock.Verify(x => x.GetAsync<PagedResponse<Vehicle>>(It.IsAny<string>(),
            It.Is<IDictionary<string, string>>(h => h["Authenticate"] == "cached-token"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Init_WithUnexpiredStoredToken_UsesIt()
    {
        _credential = _credential with { Token = "stored", TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(10) };

        await DeviceReader.Init(_credential, TestCancellationToken);

        HttpClientServiceMock.Verify(x => x.PostAsync<LoginResponse>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Never);
        SessionStoreMock.Verify(x => x.Set(_credential, "stored", SessionHelper.SessionTtl, true), Times.Once);
    }

    [Test]
    public async Task Init_WithExpiredStoredToken_LogsIn()
    {
        _credential = _credential with { Token = "stored", TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(-1) };

        await DeviceReader.Init(_credential, TestCancellationToken);

        HttpClientServiceMock.Verify(x => x.PostAsync<LoginResponse>("login", It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Init_LoginWithoutToken_Throws()
    {
        HttpClientServiceMock.Setup(x => x.PostAsync<LoginResponse>("login", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResponse("Invalid credentials", null));

        await Assert.ThrowsAsync<InvalidOperationException>(() => DeviceReader.Init(_credential, TestCancellationToken));
    }

    [Test]
    public async Task GetDevicesAsync_PagesUntilTheLastPage_AndSkipsVehiclesWithoutDevice()
    {
        SetupVehiclePages(
            new PagedResponse<Vehicle>([new Vehicle(1, "A", 111), new Vehicle(2, "No device", null)], 1, 2),
            new PagedResponse<Vehicle>([new Vehicle(3, "C", 333)], 2, 2));
        await DeviceReader.Init(_credential, TestCancellationToken);

        var devices = (await DeviceReader.GetDevicesAsync(TestCancellationToken)).ToList();

        Assert.That(devices.Select(d => (d.Identifier, d.Serial)), Is.EqualTo(new[] { (1, "111"), (3, "333") }));
        HttpClientServiceMock.Verify(x => x.GetAsync<PagedResponse<Vehicle>>("vehicles?set=1000&page=1", It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
        HttpClientServiceMock.Verify(x => x.GetAsync<PagedResponse<Vehicle>>("vehicles?set=1000&page=2", It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetDevicesAsync_ForDevices_ReturnsOnlyTheRequestedSerials()
    {
        SetupVehiclePages(new PagedResponse<Vehicle>([new Vehicle(1, "A", 111), new Vehicle(3, "C", 333)], 1, 1));
        await DeviceReader.Init(_credential, TestCancellationToken);

        var devices = (await DeviceReader.GetDevicesAsync([CreateDeviceTransporterVm(0, "333")], TestCancellationToken)).ToList();

        Assert.That(devices.Single().Serial, Is.EqualTo("333"));
        Assert.That(devices.Single().Identifier, Is.EqualTo(3));
    }

    [Test]
    public async Task GetDeviceAsync_VehicleWithoutDevice_Throws()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<Vehicle>("vehicles/5", It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Vehicle(5, "No device", null));
        await DeviceReader.Init(_credential, TestCancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => DeviceReader.GetDeviceAsync(CreateDeviceTransporterVm(5), TestCancellationToken));
    }

    [Test]
    public async Task Ping_WithCachedSession_StillCallsTheApi()
    {
        var cached = "cached-token";
        SessionStoreMock.Setup(x => x.TryGet(It.IsAny<CredentialTokenDto>(), out cached)).Returns(true);
        var tester = new ConnectivityTester(HttpClientFactoryMock.Object, HttpClientServiceMock.Object, _credentialWriterMock.Object, SessionStoreMock.Object);

        await tester.Ping(_credential, TestCancellationToken);

        HttpClientServiceMock.Verify(x => x.GetAsync<JsonElement>("user",
            It.Is<IDictionary<string, string>>(h => h["Authenticate"] == "cached-token"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
