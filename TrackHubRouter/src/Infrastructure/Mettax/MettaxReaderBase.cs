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
using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Interfaces;

namespace TrackHub.Router.Infrastructure.Mettax;

public class MettaxReaderBase
{
    private const string TokenUrl = "v2/openapi/system/createToken";
    private const string TokenScope = "mettax-token";
    private const int Success = 0;
    private const int TokenRejected = 401;
    private static readonly TimeSpan TokenIdleLifetime = TimeSpan.FromHours(3);

    private readonly ICredentialHttpClientFactory _httpClientFactory;
    private readonly IProviderSessionStore _sessionStore;
    private CredentialTokenDto _credential;
    private HttpClient _httpClient = null!;
    private string _token = string.Empty;

    protected IHttpClientService HttpClientService { get; }

    public ProtocolType Protocol => ProtocolType.Mettax;

    protected MettaxReaderBase(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService,
        IProviderSessionStore sessionStore)
    {
        HttpClientService = httpClientService;
        _httpClientFactory = httpClientFactory;
        _sessionStore = sessionStore;
    }

    public async Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        _credential = credential;
        _httpClient = _httpClientFactory.CreateClientAsync(credential, cancellationToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpClientService.Init(_httpClient, $"{ProtocolType.Mettax}");
        UseToken(await _sessionStore.AcquireAsync(TokenScope, credential, null, CreateTokenAsync, cancellationToken));
    }

    private protected async Task<T?> PostAsync<T>(string url, object body, CancellationToken cancellationToken)
    {
        var token = _token;
        var response = await SendAsync<T>(url, body, cancellationToken);
        if (response?.Code == TokenRejected)
        {
            UseToken(await _sessionStore.AcquireAsync(TokenScope, _credential, token, CreateTokenAsync, cancellationToken));
            response = await SendAsync<T>(url, body, cancellationToken);
        }

        return response is { Code: Success }
            ? response.Data
            : throw new InvalidOperationException($"Mettax API error calling '{url}' ({response?.Code}): {response?.Msg ?? "empty response"}");
    }

    private async Task<MettaxResponse<T>?> SendAsync<T>(string url, object body, CancellationToken cancellationToken)
    {
        try
        {
            return await HttpClientService.PostAsync<MettaxResponse<T>>(url, body, cancellationToken);
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new MettaxResponse<T>(TokenRejected, exception.Message, default);
        }
    }

    private async Task<ProviderSessionGrant> CreateTokenAsync(CancellationToken cancellationToken)
    {
        var response = await HttpClientService.PostAsync<MettaxResponse<string>>(
            TokenUrl, new { apiKey = _credential.Key, apiSecret = _credential.Key2 }, cancellationToken);
        if (response is not { Code: Success, Data.Length: > 0 })
        {
            throw new InvalidOperationException($"Mettax authentication failed ({response?.Code}): {response?.Msg ?? "empty response"}");
        }

        return new ProviderSessionGrant(response.Data, TokenIdleLifetime, Sliding: true);
    }

    private void UseToken(string token)
    {
        _token = token;
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
    }
}
