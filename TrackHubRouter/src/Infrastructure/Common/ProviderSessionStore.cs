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
using TrackHub.Router.Domain.Records;

namespace TrackHub.Router.Infrastructure.Common;

// In-process TTL cache of provider session artifacts, singleton like the
// other single-instance sync state (DeviceCatalogCache, OperatorSyncBackoff). The full decrypted
// credential is kept as the entry fingerprint — value equality on the record struct — so any
// rotation of URI/username/password/keys/token invalidates the session implicitly. Concurrent
// same-credential logins (position loop vs. ping loop) both Set; last-writer-wins is harmless
// because providers allow parallel sessions and the loser's session simply idles out remotely.
public sealed class ProviderSessionStore : IProviderSessionStore
{
    private readonly ConcurrentDictionary<Guid, Entry> _entries = new();

    public bool TryGet(CredentialTokenDto credential, out string session)
    {
        session = string.Empty;
        if (!_entries.TryGetValue(credential.CredentialId, out var entry))
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        if (entry.Credential != credential || now >= entry.ExpiresAt)
        {
            _entries.TryRemove(credential.CredentialId, out _);
            return false;
        }

        if (entry.Sliding)
        {
            // Benign race: a concurrent update just re-extends the expiration.
            _entries[credential.CredentialId] = entry with { ExpiresAt = now + entry.TimeToLive };
        }

        session = entry.Session;
        return true;
    }

    public void Set(CredentialTokenDto credential, string session, TimeSpan timeToLive, bool sliding = true)
    {
        if (timeToLive <= TimeSpan.Zero)
        {
            // An already-expired artifact (e.g. a token at the tail of its lifetime) is not worth
            // storing; the next Init logs in fresh.
            return;
        }

        _entries[credential.CredentialId] =
            new Entry(credential, session, timeToLive, sliding, DateTimeOffset.UtcNow + timeToLive);
    }

    public void Invalidate(Guid credentialId)
        => _entries.TryRemove(credentialId, out _);

    private readonly record struct Entry(
        CredentialTokenDto Credential,
        string Session,
        TimeSpan TimeToLive,
        bool Sliding,
        DateTimeOffset ExpiresAt);
}
