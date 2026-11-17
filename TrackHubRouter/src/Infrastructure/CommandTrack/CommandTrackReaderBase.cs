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
using Ardalis.GuardClauses;
using Common.Domain.Enums;
using TrackHub.Router.Infrastructure.CommandTrack.Helpers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;

namespace TrackHub.Router.Infrastructure.CommandTrack;

// This class represents the base class for CommandTrack readers.
// It provides common functionality and properties for CommandTrack readers.
public abstract class CommandTrackReaderBase(ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
{
    private HttpClient? _httpClient;
    private CredentialTokenDto? _credential;
    private string? _token;

    public ProtocolType Protocol => ProtocolType.CommandTrack;

    protected IHttpClientService HttpClientService { get; } = httpClientService;
    protected IDictionary<string, string>? Header { get; private set; }

    // Initializes the CommandTrack reader with the provided credential.
    // It sets up the HTTP client, retrieves the access token, and initializes the HTTP client service.
    public async Task Init(CredentialTokenDto credential, CancellationToken cancellationToken)
    {
        Guard.Against.Null(credential, message: $"No CredentialToken configurations provided for {credential.CredentialId}");
        Guard.Against.Null(credential.Key, message: $"No Credential key found for {credential.CredentialId}");

        _credential = credential;
        _httpClient = httpClientFactory.CreateClientAsync(credential, cancellationToken);
        SetBearer(await new TokenHelper(credentialWriter, sessionStore).GetTokenAsync(_httpClient, credential, null, cancellationToken));
        Header = new Dictionary<string, string> { { "Client-ID", credential.Key } };
        HttpClientService.Init(_httpClient, $"{ProtocolType.CommandTrack}");
    }

    /// <summary>
    /// Runs a provider read and, if the provider rejects the token with 401, renews the token and
    /// retries once. Without this a token revoked server-side before its recorded expiry fails every
    /// sync for the operator until the credential is rotated by hand.
    /// </summary>
    private protected async Task<T?> WithReauthenticationAsync<T>(Func<Task<T?>> read, CancellationToken cancellationToken)
    {
        try
        {
            return await read();
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (_httpClient is null || _credential is not { } credential)
            {
                throw new InvalidOperationException("CommandTrack reader was not initialized.");
            }

            SetBearer(await new TokenHelper(credentialWriter, sessionStore).GetTokenAsync(_httpClient, credential, _token, cancellationToken));
            return await read();
        }
    }

    private void SetBearer(string token)
    {
        _token = token;
        _httpClient!.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }
}
