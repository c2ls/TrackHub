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

using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;

namespace TrackHub.Router.Infrastructure.Protrack;

/// <summary>
/// Connectivity tester for Protrack API.
/// Tests connection by authenticating and fetching device list.
/// </summary>
public sealed class ConnectivityTester(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
    : ProtrackReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IConnectivityTester
{
    /// <summary>
    /// Tests connectivity by attempting to authenticate and retrieve device list from Protrack API.
    /// </summary>
    public async Task Ping(CredentialTokenDto credential, CancellationToken cancellationToken)
    {
        await Init(credential, cancellationToken);
        // Make a simple API call to verify the access token is valid
        await HttpClientService.GetAsync<DeviceListResponse>(
            $"{BaseUrl}/api/device/list?access_token={AccessToken}", cancellationToken: cancellationToken);
    }
}
