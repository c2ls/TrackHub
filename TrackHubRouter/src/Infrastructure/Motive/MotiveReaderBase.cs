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

namespace TrackHub.Router.Infrastructure.Motive;

public class MotiveReaderBase(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
{
    internal const int PageSize = 100;

    protected IHttpClientService HttpClientService { get; } = httpClientService;

    public ProtocolType Protocol => ProtocolType.Motive;

    public virtual Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClientAsync(credential, cancellationToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("X-API-Key", credential.Key);
        httpClient.DefaultRequestHeaders.Add("X-Metric-Units", "false");
        HttpClientService.Init(httpClient, $"{ProtocolType.Motive}");
        return Task.CompletedTask;
    }

    internal async Task<IReadOnlyList<Vehicle>> GetAllVehiclesAsync(string url, CancellationToken cancellationToken)
    {
        var all = new List<Vehicle>();
        var separator = url.Contains('?') ? '&' : '?';
        for (var page = 1; ; page++)
        {
            var response = await GetVehiclesAsync($"{url}{separator}per_page={PageSize}&page_no={page}", cancellationToken);
            var vehicles = response.Vehicles!.Select(entry => entry.Vehicle).ToList();
            all.AddRange(vehicles);

            var lastPage = response.Pagination is { } pagination
                ? pagination.PageNo * pagination.PerPage >= pagination.Total
                : vehicles.Count < PageSize;
            if (lastPage || vehicles.Count == 0)
            {
                return all;
            }
        }
    }

    internal async Task<VehiclesResponse> GetVehiclesAsync(string url, CancellationToken cancellationToken)
    {
        var response = await HttpClientService.GetAsync<VehiclesResponse>(url, cancellationToken: cancellationToken);
        return response?.Vehicles is null
            ? throw new InvalidOperationException("Motive answered without a vehicles list.")
            : response;
    }

    internal static string VehicleIdsQuery(IEnumerable<string> ids)
        => string.Join("&", ids.Select(id => $"vehicle_ids[]={Uri.EscapeDataString(id)}"));
}
