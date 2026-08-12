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

namespace TrackHub.Router.Infrastructure.Wialon;

/// <summary>
/// Connectivity tester for Wialon API.
/// Init may be satisfied from the session cache, so the ping issues a minimal authenticated
/// search (1 unit) to keep the health probe a REAL provider round-trip — a cached sid must never
/// turn the PING loop into a no-op.
/// </summary>
public sealed class ConnectivityTester(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    IProviderSessionStore sessionStore)
    : WialonReaderBase(httpClientFactory, httpClientService, sessionStore), IConnectivityTester
{
    /// <summary>
    /// Tests connectivity by authenticating (or reusing the cached session) and issuing a
    /// minimal search against the Wialon API.
    /// </summary>
    public async Task Ping(CredentialTokenDto credential, CancellationToken cancellationToken)
    {
        await Init(credential, cancellationToken);
        var parameters = new
        {
            spec = new
            {
                itemsType = "avl_unit",
                propName = "sys_name",
                propValueMask = "*",
                sortType = "sys_name"
            },
            force = 0,
            flags = 1,
            from = 0,
            to = 1
        };
        await PostAsync<SearchResponse>("core/search_items", parameters, cancellationToken);
    }
}
