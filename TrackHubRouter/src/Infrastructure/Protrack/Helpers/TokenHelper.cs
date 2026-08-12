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

using System.Security.Cryptography;
using System.Text;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;

namespace TrackHub.Router.Infrastructure.Protrack.Helpers;

/// <summary>
/// Helper class for managing Protrack access tokens.
/// Protrack uses MD5-based signature authentication:
/// signature = md5(md5(password) + time)
/// The durable token copy lives on the Manager credential (UpdateTokenAsync); the in-process
/// session store is a fast path that keeps the token usable while the Manager write-back is
/// still propagating through cached operator reads.
/// </summary>
internal class TokenHelper(ICredentialWriter credentialWriter, IProviderSessionStore sessionStore)
{
    // Do not hand out a token that dies mid-sync.
    private static readonly TimeSpan ExpiryMargin = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets a valid access token, refreshing if necessary.
    /// </summary>
    public async Task<string> GetTokenAsync(
        IHttpClientService httpClientService,
        string baseUrl,
        CredentialTokenDto credential,
        CancellationToken cancellationToken)
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

        return await RefreshTokenAsync(httpClientService, baseUrl, credential, cancellationToken);
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

    /// <summary>
    /// Refreshes the access token by calling the Protrack authorization API.
    /// </summary>
    private async Task<string> RefreshTokenAsync(
        IHttpClientService httpClientService,
        string baseUrl,
        CredentialTokenDto credential,
        CancellationToken cancellationToken)
    {
        var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signature = ComputeSignature(credential.Password, time);

        var url = $"{baseUrl}/api/authorization?time={time}&account={credential.Username}&signature={signature}";
        var response = await httpClientService.GetAsync<AuthorizationResponse>(url, cancellationToken: cancellationToken);

        if (response?.Record is null || string.IsNullOrEmpty(response.Record.Access_token))
        {
            throw new InvalidOperationException("Failed to obtain access token from Protrack API");
        }

        var tokenExpiration = DateTimeOffset.UtcNow.AddSeconds(response.Record.Expires_in);

        await credentialWriter.UpdateTokenAsync(credential.CredentialId, new UpdateTokenDto(
            credential.CredentialId,
            response.Record.Access_token,
            tokenExpiration,
            null,
            null
        ), cancellationToken);

        CacheToken(credential, response.Record.Access_token, tokenExpiration);
        return response.Record.Access_token;
    }

    /// <summary>
    /// Computes the Protrack signature: md5(md5(password) + time)
    /// MD5 uses 32 lower-case characters.
    /// </summary>
    internal static string ComputeSignature(string password, long time)
    {
        var passwordMd5 = ComputeMd5Hash(password);
        return ComputeMd5Hash(passwordMd5 + time);
    }

    /// <summary>
    /// Computes the MD5 hash of a string, returning 32 lower-case hex characters.
    /// </summary>
    internal static string ComputeMd5Hash(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexStringLower(hashBytes);
    }

    private static bool IsTokenExpired(CredentialTokenDto token)
        => DateTimeOffset.UtcNow >= token.TokenExpiration;
}
