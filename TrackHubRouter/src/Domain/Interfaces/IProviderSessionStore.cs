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

namespace TrackHub.Router.Domain.Interfaces;

// In-process store of per-credential provider session artifacts (Wialon sid, Navixy hash,
// Geotab session id, CommandTrack/Protrack bearer tokens) so session-auth providers do not
// re-login on every sync/ping cycle (router-audit RA-12b). Entries are keyed by CredentialId and
// fingerprinted by the full decrypted credential: a rotated credential is a cache miss, never a
// stale session. Single-instance semantics, consistent with the SyncWorker deployment
// (IOperatorSyncLock, ExecutionIntervalManager).
public interface IProviderSessionStore
{
    /// <summary>
    /// Returns a live session for the credential, or false when there is no entry, the entry
    /// expired, or the credential material changed since the session was stored.
    /// A sliding entry has its expiration extended on every successful hit.
    /// </summary>
    bool TryGet(CredentialTokenDto credential, out string session);

    /// <summary>
    /// Stores a session for the credential. <paramref name="sliding"/> entries expire
    /// <paramref name="timeToLive"/> after their LAST use (inactivity-timeout sessions);
    /// non-sliding entries expire <paramref name="timeToLive"/> after being stored
    /// (absolute-expiry tokens).
    /// </summary>
    void Set(CredentialTokenDto credential, string session, TimeSpan timeToLive, bool sliding = true);

    /// <summary>
    /// Drops the credential's session, forcing a fresh login on the next Init. Called by
    /// providers when the remote API reports the session invalid/expired.
    /// </summary>
    void Invalidate(Guid credentialId);
}
