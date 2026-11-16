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

namespace TrackHub.Router.Infrastructure.Tests;

[TestFixture]
public class SessionRenewalTests
{
    private Mock<ICredentialHttpClientFactory> _httpClientFactory = null!;
    private Mock<IHttpClientService> _httpClientService = null!;
    private Mock<IProviderSessionStore> _sessionStore = null!;
    private Mock<ICredentialWriter> _credentialWriter = null!;
    private HttpClient _httpClient = null!;

    [SetUp]
    public void SetUp()
    {
        _httpClient = new HttpClient();
        _httpClientFactory = new Mock<ICredentialHttpClientFactory>();
        _httpClientFactory.Setup(x => x.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(() => _httpClient);
        _httpClientService = new Mock<IHttpClientService>();
        _sessionStore = new Mock<IProviderSessionStore>();
        _credentialWriter = new Mock<ICredentialWriter>();
    }

    [TearDown]
    public void TearDown() => _httpClient.Dispose();

    private static CredentialTokenDto Credential(string? token = null, DateTimeOffset? expiration = null)
        => new(Guid.NewGuid(), "https://provider.example/", "user", "secret", "key", null, token, expiration, null, null);

    private static HttpRequestException Unauthorized() => new("unauthorized", null, HttpStatusCode.Unauthorized);

    [Test]
    public async Task Wialon_InvalidSession_LogsInOnceMore_AndRetries()
    {
        _httpClientService.SetupSequence(x => x.PostAsync<Wialon.Models.LoginResponse>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Wialon.Models.LoginResponse("sid-1", "user", 1))
            .ReturnsAsync(new Wialon.Models.LoginResponse("sid-2", "user", 1));
        _httpClientService.SetupSequence(x => x.PostAsync<Wialon.Models.SearchResponse>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Wialon.Models.SearchResponse(null, 0, Error: 1))
            .ReturnsAsync(new Wialon.Models.SearchResponse([], 0));
        var reader = new Wialon.DeviceReader(_httpClientFactory.Object, _httpClientService.Object, _sessionStore.Object);
        var credential = Credential();
        await reader.Init(credential);

        await reader.GetDevicesAsync(CancellationToken.None);

        _httpClientService.Verify(x => x.PostAsync<Wialon.Models.LoginResponse>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _sessionStore.Verify(x => x.Invalidate(credential.CredentialId), Times.Once);
        _sessionStore.Verify(x => x.Set(credential, "sid-2", It.IsAny<TimeSpan>(), true), Times.Once);
    }

    [Test]
    public async Task Navixy_InvalidHash_AuthenticatesOnceMore_AndRetries()
    {
        _httpClientService.SetupSequence(x => x.PostAsync<Navixy.Models.AuthResponse>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Navixy.Models.AuthResponse(true, "hash-1"))
            .ReturnsAsync(new Navixy.Models.AuthResponse(true, "hash-2"));
        _httpClientService.Setup(x => x.PostAsync<Navixy.Models.UserSettingsResponse>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Navixy.Models.UserSettingsResponse(true, new Navixy.Models.NavixyUserSettings("UTC")));
        _httpClientService.SetupSequence(x => x.PostAsync<Navixy.Models.TrackerListResponse>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Navixy.Models.TrackerListResponse(false, null, new Navixy.Models.NavixyStatus(4, "Session not found")))
            .ReturnsAsync(new Navixy.Models.TrackerListResponse(true, []));
        var reader = new Navixy.DeviceReader(_httpClientFactory.Object, _httpClientService.Object, _sessionStore.Object);
        var credential = Credential();
        await reader.Init(credential);

        await reader.GetDevicesAsync(CancellationToken.None);

        _httpClientService.Verify(x => x.PostAsync<Navixy.Models.AuthResponse>(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _sessionStore.Verify(x => x.Invalidate(credential.CredentialId), Times.Once);
    }

    [Test]
    public async Task Protrack_RejectedToken_AuthenticatesOnceMore_WritesItBack_AndRetries()
    {
        _httpClientService.SetupSequence(x => x.GetAsync<Protrack.Models.AuthorizationResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Protrack.Models.AuthorizationResponse(0, new Protrack.Models.AuthorizationRecord("token-1", 7200)))
            .ReturnsAsync(new Protrack.Models.AuthorizationResponse(0, new Protrack.Models.AuthorizationRecord("token-2", 7200)));
        _httpClientService.SetupSequence(x => x.GetAsync<Protrack.Models.DeviceListResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(Unauthorized())
            .ReturnsAsync(new Protrack.Models.DeviceListResponse(0, []));
        var reader = new Protrack.DeviceReader(_httpClientFactory.Object, _httpClientService.Object, _credentialWriter.Object, _sessionStore.Object);
        await reader.Init(Credential());

        await reader.GetDevicesAsync(CancellationToken.None);

        _httpClientService.Verify(x => x.GetAsync<Protrack.Models.DeviceListResponse>(
            It.Is<string>(url => url.Contains("access_token=token-2")), It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()), Times.Once);
        _credentialWriter.Verify(x => x.UpdateTokenAsync(It.IsAny<Guid>(), It.IsAny<UpdateTokenDto>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task Protrack_StoredTokenAboutToExpire_IsNotReused()
    {
        _httpClientService.Setup(x => x.GetAsync<Protrack.Models.AuthorizationResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Protrack.Models.AuthorizationResponse(0, new Protrack.Models.AuthorizationRecord("token-1", 7200)));
        var reader = new Protrack.DeviceReader(_httpClientFactory.Object, _httpClientService.Object, _credentialWriter.Object, _sessionStore.Object);

        await reader.Init(Credential("stored", DateTimeOffset.UtcNow.AddSeconds(30)));

        _httpClientService.Verify(x => x.GetAsync<Protrack.Models.AuthorizationResponse>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CommandTrack_RejectedToken_AuthenticatesOnceMore_AndSendsTheNewBearer()
    {
        var handler = new RecordingHttpHandler();
        handler.EnqueueJson("""{"token":"token-1","expires":"2099-01-01T00:00:00Z"}""");
        handler.EnqueueJson("""{"token":"token-2","expires":"2099-01-01T00:00:00Z"}""");
        _httpClient = handler.CreateClient("https://commandtrack.example/");
        _httpClientService.SetupSequence(x => x.GetAsync<IEnumerable<CommandTrack.Models.DevicePosition>>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(Unauthorized())
            .ReturnsAsync([]);
        var reader = new CommandTrack.DeviceReader(_httpClientFactory.Object, _httpClientService.Object, _credentialWriter.Object, _sessionStore.Object);
        await reader.Init(Credential(), CancellationToken.None);

        await reader.GetDevicesAsync(CancellationToken.None);

        Assert.That(handler.Requests, Has.Count.EqualTo(2));
        Assert.That(_httpClient.DefaultRequestHeaders.GetValues("Authorization"), Is.EqualTo(new[] { "Bearer token-2" }));
        _credentialWriter.Verify(x => x.UpdateTokenAsync(It.IsAny<Guid>(), It.IsAny<UpdateTokenDto>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
