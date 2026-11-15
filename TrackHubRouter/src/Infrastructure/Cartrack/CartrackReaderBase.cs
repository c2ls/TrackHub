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
using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Interfaces;

namespace TrackHub.Router.Infrastructure.Cartrack;

public class CartrackReaderBase(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
{
    internal const int PageSize = 1000;

    protected IHttpClientService HttpClientService { get; } = httpClientService;

    public ProtocolType Protocol => ProtocolType.Cartrack;

    public virtual Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClientAsync(credential, cancellationToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = BasicAuthentication.Header(credential.Username, credential.Password);
        HttpClientService.Init(httpClient, $"{ProtocolType.Cartrack}");
        return Task.CompletedTask;
    }

    internal async Task<IReadOnlyList<T>> GetAllPagesAsync<T>(string url, CancellationToken cancellationToken)
    {
        var all = new List<T>();
        var separator = url.Contains('?') ? '&' : '?';
        for (var page = 1; ; page++)
        {
            var response = await GetPageAsync<T>($"{url}{separator}page={page}&limit={PageSize}", cancellationToken);
            var items = response.Data!.ToList();
            all.AddRange(items);

            var lastPage = response.Meta is { } meta ? meta.CurrentPage >= meta.LastPage : items.Count < PageSize;
            if (lastPage || items.Count == 0)
            {
                return all;
            }
        }
    }

    internal async Task<Page<T>> GetPageAsync<T>(string url, CancellationToken cancellationToken)
    {
        var response = await HttpClientService.GetAsync<Page<T>>(url, cancellationToken: cancellationToken);
        return response?.Data is null
            ? throw new InvalidOperationException("Cartrack answered without a data list.")
            : response;
    }

    internal static string RegistrationsFilter(IEnumerable<string> registrations)
        => $"filter[registrations]={string.Join(",", registrations.Select(Uri.EscapeDataString))}";
}
