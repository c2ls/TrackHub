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
using TrackHub.Router.Domain.Records;

namespace TrackHub.Router.Infrastructure.GpsGate;

public class GpsGateReaderBase
{
    private readonly ICredentialHttpClientFactory _httpClientFactory;
    protected IHttpClientService HttpClientService { get; }

    public ProtocolType Protocol => ProtocolType.GpsGate;
    protected string ApplicationId = string.Empty;
    protected string UserId = string.Empty;

    protected GpsGateReaderBase(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
    {
        HttpClientService = httpClientService;
        _httpClientFactory = httpClientFactory;
    }

    // Initializes the GpsGate reader with the provided credential.
    public virtual Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClientAsync(credential, cancellationToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("Authorization", credential.Password);
        HttpClientService.Init(httpClient, $"{ProtocolType.GpsGate}");
        ApplicationId = credential.Key ?? ApplicationId;
        UserId = credential.Key2 ?? UserId;
        return Task.CompletedTask;
    }
}
