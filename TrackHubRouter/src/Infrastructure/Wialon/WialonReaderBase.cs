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
using TrackHub.Router.Domain.Interfaces;

namespace TrackHub.Router.Infrastructure.Wialon;

/// <summary>
/// Base class for Wialon readers providing common functionality for API communication.
/// Wialon uses token-based authentication via the token/login endpoint; the resulting session id
/// (sid) is reused across sync/ping cycles through <see cref="IProviderSessionStore"/>
/// instead of re-logging-in every Init. Wialon sessions die after ~5 minutes of inactivity, so the
/// cache slides below that and an invalid-session error (1) triggers one re-login + retry.
/// </summary>
public class WialonReaderBase
{
    // Wialon "invalid session" error code; any other non-zero error is a real API failure.
    private const long InvalidSessionError = 1;

    // Wialon's default session inactivity timeout is 5 minutes; slide safely below it.
    private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(4);

    private readonly ICredentialHttpClientFactory _httpClientFactory;
    private readonly IProviderSessionStore _sessionStore;
    private CredentialTokenDto _credential;
    private string _sid = string.Empty;
    private string _baseUrl = string.Empty;

    protected IHttpClientService HttpClientService { get; }

    public ProtocolType Protocol => ProtocolType.Wialon;

    protected WialonReaderBase(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService,
        IProviderSessionStore sessionStore)
    {
        HttpClientService = httpClientService;
        _httpClientFactory = httpClientFactory;
        _sessionStore = sessionStore;
    }

    /// <summary>
    /// Initializes the Wialon reader with the provided credential, reusing a cached session id
    /// when one is live; otherwise authenticates via token/login and caches the new sid.
    /// </summary>
    public virtual async Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClientAsync(credential, cancellationToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _credential = credential;
        _baseUrl = credential.Uri.TrimEnd('/');

        HttpClientService.Init(httpClient, $"{ProtocolType.Wialon}");

        if (_sessionStore.TryGet(credential, out var cachedSid))
        {
            _sid = cachedSid;
            return;
        }

        await LoginAsync(cancellationToken);
    }

    /// <summary>
    /// Logs in with the credential token, stores the fresh session id in the session store.
    /// </summary>
    private async Task LoginAsync(CancellationToken cancellationToken)
    {
        var loginUrl = $"{_baseUrl}/wialon/ajax.html?svc=token/login";
        var loginParams = new { token = _credential.Token };
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "params", JsonSerializer.Serialize(loginParams) }
        });

        var loginResponse = await HttpClientService.PostAsync<LoginResponse>(loginUrl, content, cancellationToken);
        if (loginResponse?.Error is { } error)
        {
            throw new InvalidOperationException($"Wialon login failed with API error {error}");
        }

        _sid = string.IsNullOrEmpty(loginResponse?.Eid)
            ? throw new InvalidOperationException("Failed to obtain session ID from Wialon")
            : loginResponse.Eid;

        _sessionStore.Set(_credential, _sid, SessionTtl);
    }

    /// <summary>
    /// Makes a POST request to the Wialon API. An invalid-session error (a cached sid the server
    /// already dropped) re-logs-in and retries ONCE; any other Wialon error code throws — it must
    /// never be mistaken for an empty result (Wialon errors arrive as HTTP 200).
    /// </summary>
    private protected async Task<T?> PostAsync<T>(string svc, object parameters, CancellationToken cancellationToken)
        where T : class, IWialonResponse
    {
        var result = await PostRawAsync<T>(svc, parameters, cancellationToken);
        if (result?.Error is not { } error)
        {
            return result;
        }

        if (error != InvalidSessionError)
        {
            throw new InvalidOperationException($"Wialon API error {error} calling '{svc}'");
        }

        _sessionStore.Invalidate(_credential.CredentialId);
        await LoginAsync(cancellationToken);

        result = await PostRawAsync<T>(svc, parameters, cancellationToken);
        return result?.Error is { } retryError
            ? throw new InvalidOperationException($"Wialon API error {retryError} calling '{svc}' after re-login")
            : result;
    }

    private async Task<T?> PostRawAsync<T>(string svc, object parameters, CancellationToken cancellationToken)
        where T : class, IWialonResponse
    {
        var url = $"{_baseUrl}/wialon/ajax.html?svc={svc}&sid={_sid}";
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "params", JsonSerializer.Serialize(parameters) }
        });

        return await HttpClientService.PostAsync<T>(url, content, cancellationToken);
    }
}
