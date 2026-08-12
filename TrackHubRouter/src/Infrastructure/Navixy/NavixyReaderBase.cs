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
using Common.Domain.Enums;
using TrackHub.Router.Domain.Interfaces;

namespace TrackHub.Router.Infrastructure.Navixy;

/// <summary>
/// Base class for Navixy readers providing common functionality for API communication.
/// Navixy uses session hash authentication obtained via the user/auth endpoint; the hash is
/// reused across sync/ping cycles through <see cref="IProviderSessionStore"/> A dropped
/// session surfaces as <c>success: false</c> with status code 3/4 and triggers one re-auth + retry.
/// </summary>
public class NavixyReaderBase
{
    // Navixy status codes that mean the session hash is invalid or expired.
    private const int WrongUserHashError = 3;
    private const int SessionNotFoundError = 4;

    // Sliding reuse window for the session hash; a stale hash self-heals via re-auth + retry.
    private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(30);

    private readonly ICredentialHttpClientFactory _httpClientFactory;
    private readonly IProviderSessionStore _sessionStore;
    private CredentialTokenDto _credential;

    protected IHttpClientService HttpClientService { get; }

    public ProtocolType Protocol => ProtocolType.Navixy;

    // Navixy date format: yyyy-MM-dd HH:mm:ss
    protected const string NavixyDateFormat = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// Gets the current session hash.
    /// </summary>
    protected string Hash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the base URL for API calls.
    /// </summary>
    protected string BaseUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Formats a DateTimeOffset to Navixy date format.
    /// </summary>
    protected static string FormatNavixyDate(DateTimeOffset date)
        => date.ToString(NavixyDateFormat);

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

        if (_sessionStore.TryGet(credential, out var cachedHash))
        {
            Hash = cachedHash;
            return;
        }

        await AuthenticateAsync(cancellationToken);
    }

    /// <summary>
    /// Authenticates via user/auth, stores the fresh session hash in the session store.
    /// </summary>
    private async Task AuthenticateAsync(CancellationToken cancellationToken)
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

        _sessionStore.Set(_credential, Hash, SessionTtl);
    }

    /// <summary>
    /// Makes a POST request to the Navixy API. <paramref name="parameterFactory"/> receives the
    /// current session hash so the request can be rebuilt after a re-auth. An invalid-session
    /// status (3/4) re-authenticates and retries ONCE; any other <c>success: false</c> throws —
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

        _sessionStore.Invalidate(_credential.CredentialId);
        await AuthenticateAsync(cancellationToken);

        result = await HttpClientService.PostAsync<T>(
            $"{BaseUrl}{path}", parameterFactory(Hash), cancellationToken);
        return result is { Success: false }
            ? throw new InvalidOperationException(
                $"Navixy API error {result.Status?.Code} calling '{path}' after re-auth: {result.Status?.Description}")
            : result;
    }

    /// <summary>
    /// Parses a Navixy date string to DateTimeOffset.
    /// </summary>
    protected static DateTimeOffset ParseNavixyDate(string dateStr)
        // Navixy timestamps are naive (no zone); assume UTC and normalize to UTC.
        => DateTimeOffset.TryParseExact(dateStr, NavixyDateFormat, null,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out var result)
            ? result
            : DateTimeOffset.MinValue;
}
