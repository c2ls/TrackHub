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
using TrackHub.Router.Infrastructure.Plaspy.Mappers;

namespace TrackHub.Router.Infrastructure.Plaspy.Helpers;

internal sealed class TokenHelper(ICredentialWriter credentialWriter, IProviderSessionStore sessionStore)
{
    private static readonly TimeSpan ExpiryMargin = TimeSpan.FromMinutes(1);

    public async Task<string> GetTokenAsync(IHttpClientService httpClientService, CredentialTokenDto credential, CancellationToken cancellationToken)
    {
        if (sessionStore.TryGet(credential, out var cached))
        {
            return cached;
        }

        if (credential is { Token.Length: > 0, TokenExpiration: { } expiration } && expiration - ExpiryMargin > DateTimeOffset.UtcNow)
        {
            CacheToken(credential, credential.Token, expiration);
            return credential.Token;
        }

        return await RefreshTokenAsync(httpClientService, credential, cancellationToken);
    }

    public Task<string> ForceRefreshTokenAsync(IHttpClientService httpClientService, CredentialTokenDto credential, CancellationToken cancellationToken)
    {
        sessionStore.Invalidate(credential.CredentialId);
        return RefreshTokenAsync(httpClientService, credential, cancellationToken);
    }

    private async Task<string> RefreshTokenAsync(IHttpClientService httpClientService, CredentialTokenDto credential, CancellationToken cancellationToken)
    {
        var result = await httpClientService.PostAsync<AuthResult>(
            "api/Auth/Token", new { userName = credential.Username, apiKey = credential.Key }, cancellationToken);
        if (result is not { Success: true, Token.Length: > 0 })
        {
            throw new InvalidOperationException(
                $"Plaspy authentication failed ({result?.ErrorCode}): {result?.ErrorText ?? result?.Error}");
        }

        var expiration = result.Expires is { } expires ? PositionMapper.ParseUtc(expires) : (DateTimeOffset?)null;
        await credentialWriter.UpdateTokenAsync(credential.CredentialId,
            new UpdateTokenDto(credential.CredentialId, result.Token, expiration, null, null),
            cancellationToken);
        CacheToken(credential, result.Token, expiration);
        return result.Token;
    }

    // Without an expiry there is no TTL basis, so the token is not cached in-process.
    private void CacheToken(CredentialTokenDto credential, string token, DateTimeOffset? expiration)
    {
        if (expiration - ExpiryMargin - DateTimeOffset.UtcNow is { } timeToLive && timeToLive > TimeSpan.Zero)
        {
            sessionStore.Set(credential, token, timeToLive, sliding: false);
        }
    }
}
