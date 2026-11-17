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

namespace TrackHub.Router.Infrastructure.GpsTrackit;

public class GpsTrackitReaderBase(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
{
    internal const string UnitSearchUrl = "unit/search";
    internal const int UnitPageSize = 100;
    private const string RequestScope = "gpstrackit-request";
    private Guid _credentialId;

    internal static TimeSpan MinRequestInterval { get; set; } = TimeSpan.FromSeconds(10);

    protected IHttpClientService HttpClientService => httpClientService;

    public ProtocolType Protocol => ProtocolType.GpsTrackit;

    public Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        _credentialId = credential.CredentialId;
        var httpClient = httpClientFactory.CreateClientAsync(credential, cancellationToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("x-api-key", credential.Key ?? string.Empty);
        HttpClientService.Init(httpClient, $"{ProtocolType.GpsTrackit}");
        return Task.CompletedTask;
    }

    private protected async Task<IReadOnlyList<Unit>> SearchUnitsAsync(CancellationToken cancellationToken)
        => await SearchAllAsync<Unit>(UnitSearchUrl, offset => new { openSearch = string.Empty, limit = UnitPageSize, offset }, cancellationToken);

    private protected async Task<IReadOnlyList<T>> SearchAllAsync<T>(string url, Func<int, object> body, CancellationToken cancellationToken)
    {
        var items = new List<T>();
        while (true)
        {
            var page = await SearchAsync<T>(url, body(items.Count), cancellationToken, notFoundIsEmpty: true);
            var pageItems = page?.Items ?? [];
            items.AddRange(pageItems);
            if (pageItems.Count == 0 || page?.Total is not { } total || items.Count >= total)
            {
                return items;
            }
        }
    }

    private protected Task<SearchPage<T>?> SearchAsync<T>(string url, object body, CancellationToken cancellationToken, bool notFoundIsEmpty)
        => RequestPacing.RunAsync(RequestScope, _credentialId, MinRequestInterval, async () =>
        {
            try
            {
                return await HttpClientService.PostAsync<SearchPage<T>>(url, body, cancellationToken);
            }
            catch (HttpRequestException exception) when (notFoundIsEmpty && exception.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }, cancellationToken);
}
