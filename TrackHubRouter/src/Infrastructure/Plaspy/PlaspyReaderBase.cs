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
using System.Net.Http.Headers;
using Common.Domain.Enums;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Infrastructure.Plaspy.Helpers;

namespace TrackHub.Router.Infrastructure.Plaspy;

public class PlaspyReaderBase
{
    private readonly ICredentialHttpClientFactory _httpClientFactory;
    private readonly TokenHelper _tokenHelper;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private CredentialTokenDto _credential;
    private HttpClient _httpClient = null!;
    private string _token = string.Empty;

    protected IHttpClientService HttpClientService { get; }

    public ProtocolType Protocol => ProtocolType.Plaspy;

    protected PlaspyReaderBase(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService,
        ICredentialWriter credentialWriter,
        IProviderSessionStore sessionStore)
    {
        HttpClientService = httpClientService;
        _httpClientFactory = httpClientFactory;
        _tokenHelper = new TokenHelper(credentialWriter, sessionStore);
    }

    public async Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        _credential = credential;
        _httpClient = _httpClientFactory.CreateClientAsync(credential, cancellationToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpClientService.Init(_httpClient, $"{ProtocolType.Plaspy}");
        _token = await _tokenHelper.GetTokenAsync(HttpClientService, credential, cancellationToken);
    }

    private protected Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
        where T : class, IPlaspyResponse
        => SendAsync(url, token => HttpClientService.GetAsync<T>(url, Bearer(token), cancellationToken), cancellationToken);

    private protected Task<T> PostAsync<T>(string url, object body, CancellationToken cancellationToken)
        where T : class, IPlaspyResponse
        => SendAsync(url, token =>
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return HttpClientService.PostAsync<T>(url, body, cancellationToken);
        }, cancellationToken);

    // Plaspy reports failures as success:false with HTTP 200; those must never pass as an empty result.
    private async Task<T> SendAsync<T>(string url, Func<string, Task<T?>> send, CancellationToken cancellationToken)
        where T : class, IPlaspyResponse
    {
        var token = _token;
        T? result;
        try
        {
            result = await send(token);
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            result = await send(await RefreshTokenAsync(token, cancellationToken));
        }

        return result is null or { Success: false }
            ? throw new InvalidOperationException($"Plaspy API error calling '{url}': {result?.Error ?? "empty response"}")
            : result;
    }

    // Position reads run concurrently: the token travels per request, and readers rejected together refresh it once.
    private async Task<string> RefreshTokenAsync(string rejectedToken, CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (_token == rejectedToken)
            {
                _token = await _tokenHelper.ForceRefreshTokenAsync(HttpClientService, _credential, cancellationToken);
            }
            return _token;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static Dictionary<string, string> Bearer(string token) => new() { ["Authorization"] = $"Bearer {token}" };
}
