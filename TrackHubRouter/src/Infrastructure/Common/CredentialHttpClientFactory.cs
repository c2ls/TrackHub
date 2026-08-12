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

using TrackHub.Router.Domain.Records;

namespace TrackHub.Router.Infrastructure.Common;

// This class represents a factory for creating HttpClient instances with credentials.
public sealed class CredentialHttpClientFactory(IHttpClientFactory httpClientFactory) : ICredentialHttpClientFactory
{
    // The provider clients share one hardened named client (registered in AddCommonContext) whose
    // primary handler disables auto-redirect, so an operator-controlled base URL cannot 302-redirect
    // the Router to an internal endpoint (router-audit A-20). The base URL is applied per instance.
    public const string ProviderHttpClientName = "ProviderCredentialClient";

    // Creates a new HttpClient instance with the specified credential and cancellation token.
    // If the credential has a valid URI, sets the BaseAddress and Timeout properties of the HttpClient.
    // Otherwise, throws an InvalidOperationException.
    public HttpClient CreateClientAsync(CredentialTokenDto credential, CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient(ProviderHttpClientName);
        if (!string.IsNullOrEmpty(credential.Uri))
        {
            httpClient.BaseAddress = AsBaseAddress(credential.Uri);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }
        else
        {
            throw new InvalidOperationException($"Base URL for client '{credential.CredentialId}' not initialized.");
        }
        return httpClient;
    }

    // Providers that dial relative paths (Traccar "api/server", CommandTrack
    // "DataConnectAPI/api/...") resolve them against BaseAddress, and .NET Uri resolution drops
    // the last segment of a base whose path lacks a trailing slash — "https://host/gps" + "api/x"
    // resolves to "https://host/api/x". The credential Uri is operator-entered text, so the
    // trailing slash is established here, in the one component that depends on it, rather than
    // demanded of whoever filled in the form. Any query or fragment is dropped: relative
    // resolution discards it regardless, so a base address never carried one meaningfully.
    private static Uri AsBaseAddress(string uri)
    {
        var parsed = new Uri(uri);
        return parsed.AbsolutePath.EndsWith('/')
            ? parsed
            : new Uri(parsed, $"{parsed.AbsolutePath}/");
    }
}
