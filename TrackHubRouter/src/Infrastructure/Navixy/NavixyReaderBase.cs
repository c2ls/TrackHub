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

using System.Net.Http.Headers;
using System.Text.Json;
using Common.Domain.Enums;
using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Interfaces;

namespace TrackHub.Router.Infrastructure.Navixy;

/// <summary>
/// Base class for Navixy readers providing common functionality for API communication.
/// Navixy uses session hash authentication obtained via the user/auth endpoint; the hash is
/// reused, with the user's time zone (Navixy v2 reads and writes every date in it), across sync/ping cycles through <see cref="IProviderSessionStore"/> A dropped
/// session surfaces as <c>success: false</c> with status code 3/4 and triggers one re-auth + retry.
/// </summary>
public class NavixyReaderBase
{
    // Navixy status codes that mean the session hash is invalid or expired.
    private const int WrongUserHashError = 3;
    private const int SessionNotFoundError = 4;
    private const string SessionScope = "navixy-session";

    // Sliding reuse window for the session hash; a stale hash self-heals via re-auth + retry.
    private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(30);

    private readonly ICredentialHttpClientFactory _httpClientFactory;
    private readonly IProviderSessionStore _sessionStore;
    private CredentialTokenDto _credential;
    private string _session = string.Empty;

    protected IHttpClientService HttpClientService { get; }

    public ProtocolType Protocol => ProtocolType.Navixy;

    /// <summary>
    /// Gets the current session hash.
    /// </summary>
    protected string Hash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the base URL for API calls.
    /// </summary>
    protected string BaseUrl { get; private set; } = string.Empty;

    protected TimeZoneInfo UserTimeZone { get; private set; } = TimeZoneInfo.Utc;

    protected NavixyReaderBase(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService,
        IProviderSessionStore sessionStore)
    {
        HttpClientService = httpClientService;
        _httpClientFactory = httpClientFactory;
        _sessionStore = sessionStore;
    }

    /// <summary>
    /// Initializes the Navixy reader with the provided credential, reusing a cached session hash
    /// when one is live; otherwise authenticates via user/auth and caches the new hash.
    /// </summary>
    public virtual async Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClientAsync(credential, cancellationToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _credential = credential;
        BaseUrl = credential.Uri.TrimEnd('/');

        HttpClientService.Init(httpClient, $"{ProtocolType.Navixy}");

        await AcquireSessionAsync(null, cancellationToken);
    }

    private async Task AcquireSessionAsync(string? rejectedSession, CancellationToken cancellationToken)
    {
        _session = await _sessionStore.AcquireAsync(SessionScope, _credential, rejectedSession, AuthenticateAsync, cancellationToken);
        if (TryRestoreSession(_session))
        {
            return;
        }

        _session = await _sessionStore.AcquireAsync(SessionScope, _credential, _session, AuthenticateAsync, cancellationToken);
        if (!TryRestoreSession(_session))
        {
            throw new InvalidOperationException("Navixy returned a session that cannot be restored.");
        }
    }

    /// <summary>
    /// Authenticates via user/auth and reads the user time zone the session carries.
    /// </summary>
    private async Task<ProviderSessionGrant> AuthenticateAsync(CancellationToken cancellationToken)
    {
        var authUrl = $"{BaseUrl}/v2/user/auth";
        var authResponse = await HttpClientService.PostAsync<AuthResponse>(authUrl,
            new { login = _credential.Username, password = _credential.Password }, cancellationToken);

        if (authResponse is { Success: false })
        {
            throw new InvalidOperationException(
                $"Navixy authentication failed with status {authResponse.Status?.Code}: {authResponse.Status?.Description}");
        }

        Hash = string.IsNullOrEmpty(authResponse?.Hash)
            ? throw new InvalidOperationException("Failed to obtain session hash from Navixy")
            : authResponse.Hash;

        UserTimeZone = await ReadUserTimeZoneAsync(cancellationToken);
        return new ProviderSessionGrant(JsonSerializer.Serialize(new NavixySession(Hash, UserTimeZone.Id)), SessionTtl, Sliding: true);
    }

    private async Task<TimeZoneInfo> ReadUserTimeZoneAsync(CancellationToken cancellationToken)
    {
        var response = await HttpClientService.PostAsync<UserSettingsResponse>(
            $"{BaseUrl}/v2/user/settings/get", new { hash = Hash }, cancellationToken);
        var zoneId = response is { Success: true, Settings.Time_zone: { Length: > 0 } id }
            ? id
            : throw new InvalidOperationException(
                $"Navixy user settings carry no time zone (status {response?.Status?.Code}: {response?.Status?.Description})");

        return TimeZoneInfo.TryFindSystemTimeZoneById(zoneId, out var zone)
            ? zone
            : throw new InvalidOperationException($"Navixy user time zone '{zoneId}' is not a known zone.");
    }

    private bool TryRestoreSession(string cached)
    {
        NavixySession? session;
        try
        {
            session = JsonSerializer.Deserialize<NavixySession>(cached);
        }
        catch (JsonException)
        {
            return false;
        }

        if (session is not { Hash.Length: > 0 } || !TimeZoneInfo.TryFindSystemTimeZoneById(session.TimeZone, out var zone))
        {
            return false;
        }

        Hash = session.Hash;
        UserTimeZone = zone;
        return true;
    }

    private sealed record NavixySession(string Hash, string TimeZone);


    /// <summary>
    /// Makes a POST request to the Navixy API. <paramref name="parameterFactory"/> receives the
    /// current session hash so the request can be rebuilt after a re-auth. An invalid-session
    /// status (3/4) renews the session and retries ONCE; any other <c>success: false</c> throws —
    /// it must never be mistaken for an empty result (Navixy errors arrive as HTTP 200).
    /// </summary>
    private protected async Task<T?> PostNavixyAsync<T>(
        string path,
        Func<string, object> parameterFactory,
        CancellationToken cancellationToken)
        where T : class, INavixyResponse
    {
        var result = await HttpClientService.PostAsync<T>(
            $"{BaseUrl}{path}", parameterFactory(Hash), cancellationToken);
        if (result is not { Success: false })
        {
            return result;
        }

        if (result.Status?.Code is not (WrongUserHashError or SessionNotFoundError))
        {
            throw new InvalidOperationException(
                $"Navixy API error {result.Status?.Code} calling '{path}': {result.Status?.Description}");
        }

        await AcquireSessionAsync(_session, cancellationToken);

        result = await HttpClientService.PostAsync<T>(
            $"{BaseUrl}{path}", parameterFactory(Hash), cancellationToken);
        return result is { Success: false }
            ? throw new InvalidOperationException(
                $"Navixy API error {result.Status?.Code} calling '{path}' after re-auth: {result.Status?.Description}")
            : result;
    }
}
