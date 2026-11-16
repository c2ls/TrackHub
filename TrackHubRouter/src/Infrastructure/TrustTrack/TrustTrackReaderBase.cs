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

namespace TrackHub.Router.Infrastructure.TrustTrack;

public class TrustTrackReaderBase(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
{
    internal const int PageLimit = 1000;
    private string _apiKey = string.Empty;

    protected IHttpClientService HttpClientService => httpClientService;

    public ProtocolType Protocol => ProtocolType.TrustTrack;

    public Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClientAsync(credential, cancellationToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpClientService.Init(httpClient, $"{ProtocolType.TrustTrack}");
        _apiKey = credential.Key ?? string.Empty;
        return Task.CompletedTask;
    }

    private protected async Task<T> GetAsync<T>(string path, string query, CancellationToken cancellationToken)
        where T : class
        => await HttpClientService.GetAsync<T>($"{path}?{query}&api_key={Uri.EscapeDataString(_apiKey)}", null, cancellationToken)
            ?? throw new InvalidOperationException($"TrustTrack returned an empty response for '{path}'.");

    private protected async Task<IReadOnlyList<T>> GetAllPagesAsync<T>(string path, string query, CancellationToken cancellationToken)
    {
        var entries = new List<T>();
        string? continuationToken = null;
        while (true)
        {
            var pageQuery = $"{query}&limit={PageLimit}"
                + (continuationToken is null ? string.Empty : $"&continuation_token={Uri.EscapeDataString(continuationToken)}");
            var page = await GetAsync<ContinuationPage<T>>(path, pageQuery, cancellationToken);
            entries.AddRange(page.Entries);
            if (page.Entries.Count == 0 || page.ContinuationToken is null)
            {
                return entries;
            }
            if (page.ContinuationToken == continuationToken)
            {
                throw new InvalidOperationException($"TrustTrack paging of '{path}' did not advance past '{continuationToken}'.");
            }
            continuationToken = page.ContinuationToken;
        }
    }
}
