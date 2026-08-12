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

using System.Text;
using System.Text.Json;
using Ardalis.GuardClauses;
using Common.Domain.Extensions;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;

namespace TrackHub.Router.Infrastructure.CommandTrack.Helpers;
// Helper class for managing tokens. The durable copy lives on the Manager credential
// (UpdateTokenAsync); the in-process session store is a fast path that keeps the token usable
// while the Manager write-back is still propagating through cached operator reads.
internal class TokenHelper(ICredentialWriter credentialWriter, IProviderSessionStore sessionStore)
{
    // Do not hand out a token that dies mid-sync.
    private static readonly TimeSpan ExpiryMargin = TimeSpan.FromMinutes(1);

    // Retrieves a token asynchronously
    public async Task<string> GetTokenAsync(HttpClient httpClient,
        CredentialTokenDto credential,
        CancellationToken token)
    {
        if (sessionStore.TryGet(credential, out var cachedToken))
        {
            return cachedToken;
        }

        if (!string.IsNullOrEmpty(credential.Token) && !IsTokenExpired(credential))
        {
            CacheToken(credential, credential.Token, credential.TokenExpiration);
            return credential.Token;
        }

        return await RefreshTokenAsync(httpClient, credential, token);
    }

    // Refreshes the token asynchronously
    private async Task<string> RefreshTokenAsync(HttpClient httpClient, CredentialTokenDto credential, CancellationToken token)
    {
        Guard.Against.Null(credential.Key, message: "Credential key not found.");

        var model = new AuthenticateModel
        {
            Username = credential.Username,
            Password = credential.Password,
            UniqueId = credential.Key
        };

        var response = await httpClient.PostAsync("SecurityApi/Auth/authenticate", new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json"), token);
        response.EnsureSuccessStatusCode();

        var tokenResponseContent = await response.Content.ReadAsStringAsync(token);
        var newToken = tokenResponseContent.Deserialize<TokenResult>();

        if (newToken.Token is null)
        {
            // Typed rather than a raw Exception (rules.md forbids raw throws): the provider
            // authenticated but returned no token. Surfaces through the sync pipeline as a FAILED
            // run.
            throw new InvalidOperationException("CommandTrack authentication did not return a token.");
        }

        await credentialWriter.UpdateTokenAsync(credential.CredentialId, new UpdateTokenDto(
                credential.CredentialId,
                newToken.Token,
                newToken.Expires,
                null,
                null
            ), token);

        CacheToken(credential, newToken.Token, newToken.Expires);
        return newToken.Token;
    }

    // Non-sliding: a bearer token has an absolute expiry regardless of use. Tokens without an
    // expiry are not cached (no TTL basis) — the durable credential copy covers them.
    private void CacheToken(CredentialTokenDto credential, string tokenValue, DateTimeOffset? expiration)
    {
        if (expiration is { } expiresAt)
        {
            sessionStore.Set(credential, tokenValue, expiresAt - DateTimeOffset.UtcNow - ExpiryMargin, sliding: false);
        }
    }

    // Checks if the token has expired
    private static bool IsTokenExpired(CredentialTokenDto token)
        => DateTimeOffset.UtcNow >= token.TokenExpiration;
}
