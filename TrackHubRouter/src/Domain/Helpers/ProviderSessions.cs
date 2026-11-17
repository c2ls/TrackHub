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

using System.Collections.Concurrent;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Records;

namespace TrackHub.Router.Domain.Helpers;

public readonly record struct ProviderSessionGrant(string Value, TimeSpan? TimeToLive, bool Sliding = false);

public static class ProviderSessions
{
    private static readonly ConcurrentDictionary<(string Scope, Guid CredentialId), (CredentialTokenDto Credential, string Session)> LastIssued = new();

    public static readonly TimeSpan DefaultExpiryMargin = TimeSpan.FromMinutes(1);

    public static ProviderSessionGrant ExpiringAt(string value, DateTimeOffset? expiresAt, TimeSpan? margin = null)
        => new(value, expiresAt - (margin ?? DefaultExpiryMargin) - DateTimeOffset.UtcNow);

    public static bool HasLiveStoredToken(CredentialTokenDto credential, TimeSpan? margin = null)
        => credential is { Token.Length: > 0, TokenExpiration: { } expiresAt }
           && expiresAt - (margin ?? DefaultExpiryMargin) > DateTimeOffset.UtcNow;

    public static Task<string> AcquireReusingStoredTokenAsync(
        this IProviderSessionStore sessionStore,
        string scope,
        CredentialTokenDto credential,
        string? rejectedSession,
        Func<CancellationToken, Task<ProviderSessionGrant>> authenticate,
        CancellationToken cancellationToken,
        TimeSpan? margin = null)
        => sessionStore.AcquireAsync(scope, credential, rejectedSession, (renewing, token)
            => !renewing && HasLiveStoredToken(credential, margin)
                ? Task.FromResult(ExpiringAt(credential.Token!, credential.TokenExpiration, margin))
                : authenticate(token),
            cancellationToken);

    public static Task<string> AcquireAsync(
        this IProviderSessionStore sessionStore,
        string scope,
        CredentialTokenDto credential,
        string? rejectedSession,
        Func<CancellationToken, Task<ProviderSessionGrant>> create,
        CancellationToken cancellationToken)
        => sessionStore.AcquireAsync(scope, credential, rejectedSession, (_, token) => create(token), cancellationToken);

    public static Task<string> AcquireAsync(
        this IProviderSessionStore sessionStore,
        string scope,
        CredentialTokenDto credential,
        string? rejectedSession,
        Func<bool, CancellationToken, Task<ProviderSessionGrant>> create,
        CancellationToken cancellationToken)
        => CredentialGate.RunAsync(scope, credential.CredentialId, async () =>
        {
            if (sessionStore.TryGet(credential, out var cached) && cached != rejectedSession)
            {
                return cached;
            }

            var key = (scope, credential.CredentialId);
            var renewing = rejectedSession is not null;
            if (renewing)
            {
                if (LastIssued.TryGetValue(key, out var issued) && issued.Credential == credential && issued.Session != rejectedSession)
                {
                    return issued.Session;
                }
                sessionStore.Invalidate(credential.CredentialId);
            }

            var grant = await create(renewing, cancellationToken);
            if (grant.TimeToLive is { } timeToLive)
            {
                sessionStore.Set(credential, grant.Value, timeToLive, grant.Sliding);
            }
            LastIssued[key] = (credential, grant.Value);
            return grant.Value;
        }, cancellationToken);
}
