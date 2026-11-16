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
using TrackHub.Router.Infrastructure.Pegasus.Helpers;

namespace TrackHub.Router.Infrastructure.Pegasus;

public class PegasusReaderBase
{
    internal const int PageSize = 1000;
    internal const string LatestSelect = "device:latest.loc+latest.ios+latest.counters";

    private readonly ICredentialHttpClientFactory _httpClientFactory;
    private readonly SessionHelper _sessionHelper;
    private CredentialTokenDto _credential;
    private string _token = string.Empty;

    protected IHttpClientService HttpClientService { get; }

    public ProtocolType Protocol => ProtocolType.Pegasus;

    protected PegasusReaderBase(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService,
        ICredentialWriter credentialWriter,
        IProviderSessionStore sessionStore)
    {
        HttpClientService = httpClientService;
        _httpClientFactory = httpClientFactory;
        _sessionHelper = new SessionHelper(credentialWriter, sessionStore);
    }

    public async Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        _credential = credential;
        var httpClient = _httpClientFactory.CreateClientAsync(credential, cancellationToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpClientService.Init(httpClient, $"{ProtocolType.Pegasus}");
        _token = await _sessionHelper.GetTokenAsync(HttpClientService, credential, null, cancellationToken);
    }

    // Only a 401 is retried. A 429 must surface: Pegasus bans the IP of clients that keep calling after one.
    private protected async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        try
        {
            return await HttpClientService.GetAsync<T>(url, AuthenticationHeader(), cancellationToken);
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            _token = await _sessionHelper.GetTokenAsync(HttpClientService, _credential, _token, cancellationToken);
            return await HttpClientService.GetAsync<T>(url, AuthenticationHeader(), cancellationToken);
        }
    }

    private protected async Task<List<T>> GetAllVehiclesAsync<T>(string? select, CancellationToken cancellationToken)
    {
        var vehicles = new List<T>();
        var page = 1;
        PagedResponse<T>? result;
        do
        {
            var url = $"vehicles?set={PageSize}&page={page}" + (select is null ? string.Empty : $"&select={select}");
            result = await GetAsync<PagedResponse<T>>(url, cancellationToken);
            vehicles.AddRange(result?.Data ?? throw new InvalidOperationException("Pegasus answered without a vehicle list."));
            page++;
        }
        while (page <= result.Pages);

        return vehicles;
    }

    private Dictionary<string, string> AuthenticationHeader() => new() { ["Authenticate"] = _token };
}
