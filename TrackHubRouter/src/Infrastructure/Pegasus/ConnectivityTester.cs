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

using System.Text.Json;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;

namespace TrackHub.Router.Infrastructure.Pegasus;

public sealed class ConnectivityTester(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
    : PegasusReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IConnectivityTester
{
    public async Task Ping(CredentialTokenDto credential, CancellationToken cancellationToken)
    {
        await Init(credential, cancellationToken);
        await GetAsync<JsonElement>("user", cancellationToken);
    }
}
