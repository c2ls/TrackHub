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
using TrackHub.Router.Infrastructure.Protrack.Helpers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;

namespace TrackHub.Router.Infrastructure.Protrack;

/// <summary>
/// Base class for Protrack readers providing common functionality for API communication.
/// Protrack uses access token authentication obtained via MD5-based signature.
/// </summary>
public class ProtrackReaderBase
{
    private readonly ICredentialHttpClientFactory _httpClientFactory;
    private readonly TokenHelper _tokenHelper;

    protected IHttpClientService HttpClientService { get; }

    // Protrack protocol type (value 9, not yet in the shared enum)
    public ProtocolType Protocol => ProtocolType.Protrack;

    /// <summary>
    /// Gets the current access token.
    /// </summary>
    protected string AccessToken { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the base URL for API calls.
    /// </summary>
    protected string BaseUrl { get; private set; } = string.Empty;

    protected ProtrackReaderBase(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService,
        ICredentialWriter credentialWriter,
        IProviderSessionStore sessionStore)
    {
        HttpClientService = httpClientService;
        _httpClientFactory = httpClientFactory;
        _tokenHelper = new TokenHelper(credentialWriter, sessionStore);
    }

    /// <summary>
    /// Initializes the Protrack reader with the provided credential.
    /// Authenticates using MD5-based signature to obtain an access token.
    /// </summary>
    public virtual async Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClientAsync(credential, cancellationToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        BaseUrl = credential.Uri.TrimEnd('/');

        HttpClientService.Init(httpClient, $"{ProtocolType.Protrack}");

        AccessToken = await _tokenHelper.GetTokenAsync(
            HttpClientService, BaseUrl, credential, cancellationToken);
    }
}
