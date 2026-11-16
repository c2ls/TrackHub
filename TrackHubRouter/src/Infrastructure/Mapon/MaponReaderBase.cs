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

namespace TrackHub.Router.Infrastructure.Mapon;

public class MaponReaderBase(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
{
    internal const string UnitListUrl = "unit/list.json";
    private string _apiKey = string.Empty;

    protected IHttpClientService HttpClientService => httpClientService;

    public ProtocolType Protocol => ProtocolType.Mapon;

    public Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClientAsync(credential, cancellationToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpClientService.Init(httpClient, $"{ProtocolType.Mapon}");
        _apiKey = credential.Key ?? string.Empty;
        return Task.CompletedTask;
    }

    private protected async Task<IReadOnlyList<Unit>> ListUnitsAsync(string query, CancellationToken cancellationToken)
        => (await GetAsync<UnitList>(UnitListUrl, query, cancellationToken)).Units ?? [];

    private protected async Task<T> GetAsync<T>(string path, string query, CancellationToken cancellationToken)
        where T : class
    {
        var response = await HttpClientService.GetAsync<MaponResponse<T>>(
            $"{path}?key={Uri.EscapeDataString(_apiKey)}{query}", null, cancellationToken);
        return response is { Error: null, Data: { } data }
            ? data
            : throw new InvalidOperationException(
                $"Mapon API error calling '{path}' ({response?.Error?.Code}): {response?.Error?.Msg ?? "empty response"}");
    }
}
