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

namespace TrackHub.Router.Infrastructure.Pegasus.Helpers;

internal sealed class SessionHelper(ICredentialWriter credentialWriter, IProviderSessionStore sessionStore)
{
    // Pegasus drops a session after 60 minutes without use; stay inside that window.
    internal static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(50);

    public async Task<string> GetTokenAsync(IHttpClientService httpClientService, CredentialTokenDto credential, CancellationToken cancellationToken)
    {
        if (sessionStore.TryGet(credential, out var cached))
        {
            return cached;
        }

        if (credential is { Token.Length: > 0, TokenExpiration: { } expiration } && expiration > DateTimeOffset.UtcNow)
        {
            sessionStore.Set(credential, credential.Token, SessionTtl);
            return credential.Token;
        }

        return await LoginAsync(httpClientService, credential, cancellationToken);
    }

    public Task<string> ForceLoginAsync(IHttpClientService httpClientService, CredentialTokenDto credential, CancellationToken cancellationToken)
    {
        sessionStore.Invalidate(credential.CredentialId);
        return LoginAsync(httpClientService, credential, cancellationToken);
    }

    private async Task<string> LoginAsync(IHttpClientService httpClientService, CredentialTokenDto credential, CancellationToken cancellationToken)
    {
        var response = await httpClientService.PostAsync<LoginResponse>(
            "login", new { username = credential.Username, password = credential.Password }, cancellationToken);
        var token = response?.Auth is { Length: > 0 } auth
            ? auth
            : throw new InvalidOperationException($"Pegasus login returned no session token: {response?.Message}");

        await credentialWriter.UpdateTokenAsync(credential.CredentialId,
            new UpdateTokenDto(credential.CredentialId, token, DateTimeOffset.UtcNow + SessionTtl, null, null),
            cancellationToken);
        sessionStore.Set(credential, token, SessionTtl);
        return token;
    }
}
