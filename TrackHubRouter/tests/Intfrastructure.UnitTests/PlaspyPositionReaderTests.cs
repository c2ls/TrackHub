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
using TrackHub.Router.Domain.Exceptions;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.Plaspy.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Plaspy.Tests;

[TestFixture]
public class PositionReaderTests : PositionReaderTestsBase<PositionReader>
{
    private readonly CredentialTokenDto _credential = new(Guid.NewGuid(), "https://api.plaspy.com", "user", "unused", "api-key", null, null, null, null, null);
    private Mock<ICredentialWriter> _credentialWriterMock = null!;
    private HttpClient _httpClient = null!;

    protected override PositionReader CreatePositionReader(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
    {
        _credentialWriterMock = new Mock<ICredentialWriter>();
        return new PositionReader(httpClientFactory, httpClientService, _credentialWriterMock.Object, SessionStoreMock.Object);
    }

    [SetUp]
    public async Task SetUp()
    {
        _httpClient = new HttpClient();
        HttpClientFactoryMock.Setup(x => x.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(_httpClient);
        SetupAuth(new AuthResult(true, null, null, null, "jwt-1", "2099-01-01T00:00:00Z"));
        await PositionReader.Init(_credential, TestCancellationToken);
    }

    [TearDown]
    public void TearDown() => _httpClient.Dispose();

    private void SetupAuth(AuthResult result)
        => HttpClientServiceMock.Setup(x => x.PostAsync<AuthResult>("api/Auth/Token", It.IsAny<object?>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);

    private void SetupLastLocation(string serial, Func<Task<LastLocationResponse?>> respond)
        => HttpClientServiceMock.Setup(x => x.GetAsync<LastLocationResponse>($"api/devices/{serial}/lastLocation", It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(respond);

    private static LastLocationResponse Located(double latitude)
        => new(true, null, new Location("2026-03-01T10:00:00Z", latitude, -74, 30, 90, null, null));

    [Test]
    public void Init_AuthenticatesWithUserNameAndApiKey_AndSendsTheBearerToken()
    {
        HttpClientServiceMock.Verify(x => x.PostAsync<AuthResult>("api/Auth/Token",
            It.Is<object?>(body => JsonSerializer.Serialize(body, (JsonSerializerOptions?)null) == "{\"userName\":\"user\",\"apiKey\":\"api-key\"}"),
            It.IsAny<CancellationToken>()), Times.Once);
        _credentialWriterMock.Verify(x => x.UpdateTokenAsync(_credential.CredentialId,
            It.Is<UpdateTokenDto>(t => t.Token == "jwt-1" && t.TokenExpiration == new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            It.IsAny<CancellationToken>()), Times.Once);
        SessionStoreMock.Verify(x => x.Set(_credential, "jwt-1", It.Is<TimeSpan>(ttl => ttl > TimeSpan.Zero), false), Times.Once);
    }

    [Test]
    public async Task Init_FailedAuthentication_Throws()
    {
        SetupAuth(new AuthResult(false, 401, "Invalid API key or username.", null, null, null));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => PositionReader.Init(_credential, TestCancellationToken));
        Assert.That(exception!.Message, Does.Contain("Invalid API key or username."));
    }

    [Test]
    public async Task Init_WithUnexpiredStoredToken_DoesNotAuthenticate()
    {
        HttpClientServiceMock.Invocations.Clear();

        await PositionReader.Init(_credential with { Token = "stored", TokenExpiration = DateTimeOffset.UtcNow.AddHours(1) }, TestCancellationToken);

        HttpClientServiceMock.Verify(x => x.PostAsync<AuthResult>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetDevicePositionAsync_ErrorPayload_Throws()
    {
        SetupLastLocation("dev-1", () => Task.FromResult<LastLocationResponse?>(new LastLocationResponse(false, "Device not found", null)));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(serial: "dev-1"), TestCancellationToken));
        Assert.That(exception!.Message, Does.Contain("Device not found"));
    }

    [Test]
    public async Task GetDevicePositionAsync_NoLocation_Throws()
    {
        SetupLastLocation("dev-1", () => Task.FromResult<LastLocationResponse?>(new LastLocationResponse(true, null, null)));

        await Assert.ThrowsAsync<InvalidOperationException>(() => PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(serial: "dev-1"), TestCancellationToken));
    }

    [Test]
    public async Task GetDevicePositionAsync_DeviceIdIsEscapedInThePath()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<LastLocationResponse>("api/devices/a%2Fb/lastLocation", It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Located(1));

        var position = await PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(serial: "a/b"), TestCancellationToken);

        Assert.That(position.Latitude, Is.EqualTo(1));
    }

    [Test]
    public async Task GetDevicePositionAsync_ManyDevices_ReadsEachRequestedDeviceOnce_AndSkipsDevicesWithoutLocation()
    {
        SetupLastLocation("dev-1", () => Task.FromResult<LastLocationResponse?>(Located(1)));
        SetupLastLocation("dev-2", () => Task.FromResult<LastLocationResponse?>(new LastLocationResponse(true, null, null)));
        var devices = new[] { CreateDeviceTransporterVm(serial: "dev-1"), CreateDeviceTransporterVm(serial: "dev-1"), CreateDeviceTransporterVm(serial: "dev-2") };

        var positions = (await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken)).ToList();

        Assert.That(positions.Single().TransporterId, Is.EqualTo(devices[0].TransporterId));
        HttpClientServiceMock.Verify(x => x.GetAsync<LastLocationResponse>("api/devices/dev-1/lastLocation", It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetDevicePositionAsync_ManyDevices_OneFailure_ReportsAPartialReadWithTheOtherPositions()
    {
        SetupLastLocation("dev-1", () => Task.FromResult<LastLocationResponse?>(Located(1)));
        SetupLastLocation("dev-2", () => Task.FromResult<LastLocationResponse?>(new LastLocationResponse(false, "Daily limit reached", null)));
        SetupLastLocation("dev-3", () => Task.FromResult<LastLocationResponse?>(Located(3)));
        var devices = new[] { CreateDeviceTransporterVm(serial: "dev-1"), CreateDeviceTransporterVm(serial: "dev-2"), CreateDeviceTransporterVm(serial: "dev-3") };

        var exception = await Assert.ThrowsAsync<PartialPositionReadException>(() => PositionReader.GetDevicePositionAsync(devices, TestCancellationToken));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception!.Positions.Select(p => p.Latitude), Is.EquivalentTo(new[] { 1d, 3d }));
            Assert.That(exception.FailedDevices, Is.EqualTo(1));
            Assert.That(exception.TotalDevices, Is.EqualTo(3));
            Assert.That(exception.InnerException!.Message, Does.Contain("Daily limit reached"));
        }
    }

    [Test]
    public async Task GetDevicePositionAsync_ManyDevices_WithTheProviderDown_GivesUpInsteadOfTryingEveryDevice()
    {
        var calls = 0;
        HttpClientServiceMock.Setup(x => x.GetAsync<LastLocationResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                Interlocked.Increment(ref calls);
                return Task.FromException<LastLocationResponse?>(new HttpRequestException("provider down"));
            });
        var devices = Enumerable.Range(1, 60).Select(i => CreateDeviceTransporterVm(serial: $"dev-{i}")).ToArray();

        var exception = await Assert.ThrowsAsync<PartialPositionReadException>(() => PositionReader.GetDevicePositionAsync(devices, TestCancellationToken));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception!.Positions, Is.Empty);
            Assert.That(exception.FailedDevices, Is.EqualTo(devices.Length));
            Assert.That(calls, Is.LessThan(devices.Length / 2));
        }
    }

    [Test]
    public async Task Unauthorized_ReauthenticatesAndRetriesOnce()
    {
        var tokens = new List<string>();
        HttpClientServiceMock.Setup(x => x.GetAsync<LastLocationResponse>("api/devices/dev-1/lastLocation", It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns((string _, IDictionary<string, string>? headers, CancellationToken _) =>
            {
                tokens.Add(headers!["Authorization"]);
                return tokens.Count == 1
                    ? throw new HttpRequestException("expired", null, HttpStatusCode.Unauthorized)
                    : Task.FromResult<LastLocationResponse?>(Located(1));
            });
        SetupAuth(new AuthResult(true, null, null, null, "jwt-2", "2099-01-01T00:00:00Z"));

        var position = await PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(serial: "dev-1"), TestCancellationToken);

        Assert.That(position.Latitude, Is.EqualTo(1));
        Assert.That(tokens, Is.EqualTo(new[] { "Bearer jwt-1", "Bearer jwt-2" }));
        SessionStoreMock.Verify(x => x.Invalidate(_credential.CredentialId), Times.Once);
    }

    [Test]
    public async Task Unauthorized_AfterTheRetry_Throws()
    {
        SetupLastLocation("dev-1", () => throw new HttpRequestException("denied", null, HttpStatusCode.Unauthorized));

        await Assert.ThrowsAsync<HttpRequestException>(() => PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(serial: "dev-1"), TestCancellationToken));
    }

    [Test]
    public async Task GetPositionAsync_PostsTheUtcRange_AndDropsPointsOutsideIt()
    {
        var from = new DateTimeOffset(2026, 3, 1, 5, 0, 0, TimeSpan.FromHours(-5));
        var to = from.AddHours(1);
        object? body = null;
        HttpClientServiceMock.Setup(x => x.PostAsync<LocationsResponse>("api/devices/dev-1/locations", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .Callback((string _, object? parameters, CancellationToken _) => body = parameters)
            .ReturnsAsync(new LocationsResponse(true, null,
            [
                new Location("2026-03-01T09:59:59Z", 1, 1, 0, 0, null, null),
                new Location("2026-03-01T10:00:00Z", 2, 2, 0, 0, null, null),
                new Location("2026-03-01T11:00:01Z", 3, 3, 0, 0, null, null)
            ]));

        var positions = (await PositionReader.GetPositionAsync(from, to, CreateDeviceTransporterVm(serial: "dev-1"), TestCancellationToken)).ToList();

        Assert.That(positions.Select(p => p.Latitude), Is.EqualTo(new[] { 2d }));
        Assert.That(JsonSerializer.Serialize(body), Is.EqualTo("{\"from\":\"2026-03-01T10:00:00Z\",\"to\":\"2026-03-01T11:00:00Z\"}"));
    }

    [Test]
    public async Task GetPositionAsync_ErrorPayload_Throws()
    {
        HttpClientServiceMock.Setup(x => x.PostAsync<LocationsResponse>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LocationsResponse(false, "Invalid range", null));

        await Assert.ThrowsAsync<InvalidOperationException>(() => PositionReader.GetPositionAsync(DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, CreateDeviceTransporterVm(serial: "dev-1"), TestCancellationToken));
    }

    [Test]
    public async Task Ping_WithCachedToken_StillCallsTheApi()
    {
        var cached = "cached";
        SessionStoreMock.Setup(x => x.TryGet(It.IsAny<CredentialTokenDto>(), out cached)).Returns(true);
        HttpClientServiceMock.Setup(x => x.GetAsync<DevicesResponse>("api/devices", It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DevicesResponse(true, null, []));
        var tester = new ConnectivityTester(HttpClientFactoryMock.Object, HttpClientServiceMock.Object, _credentialWriterMock.Object, SessionStoreMock.Object);

        await tester.Ping(_credential, TestCancellationToken);

        HttpClientServiceMock.Verify(x => x.GetAsync<DevicesResponse>("api/devices", It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
