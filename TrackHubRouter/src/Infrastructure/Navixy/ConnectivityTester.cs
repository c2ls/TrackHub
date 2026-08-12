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

namespace TrackHub.Router.Infrastructure.Navixy;

/// <summary>
/// Connectivity tester for Navixy API.
/// Tests connection by authenticating and fetching tracker list.
/// </summary>
public sealed class ConnectivityTester(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    IProviderSessionStore sessionStore)
    : NavixyReaderBase(httpClientFactory, httpClientService, sessionStore), IConnectivityTester
{
    /// <summary>
    /// Tests connectivity by authenticating (or reusing the cached session hash) and fetching
    /// the tracker list — a REAL provider round-trip even on a session-cache hit.
    /// </summary>
    public async Task Ping(CredentialTokenDto credential, CancellationToken cancellationToken)
    {
        await Init(credential, cancellationToken);
        await PostNavixyAsync<TrackerListResponse>(
            "/v2/tracker/list", hash => new { hash }, cancellationToken);
    }
}
