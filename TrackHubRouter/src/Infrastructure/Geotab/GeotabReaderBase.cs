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

using Common.Domain.Enums;
using Geotab.Checkmate;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Records;

namespace TrackHub.Router.Infrastructure.Geotab;

// Base class for Geotab readers. The MyGeotab session id is reused across sync/ping cycles
// through IProviderSessionStore: on a cache hit the API client is constructed with
// password AND session id ("password OR sessionId are required. Both can be supplied" — SDK), so
// no authentication round-trip happens up front and the SDK transparently re-authenticates if the
// cached session has expired. Readers call PersistSession() after successful calls so an
// SDK-side re-auth rotates the cached id instead of leaving it stale.
public abstract class GeotabReaderBase(IProviderSessionStore sessionStore)
{
    // MyGeotab sessions live for days; the sliding window just bounds how long an idle
    // credential's session lingers in memory.
    private static readonly TimeSpan SessionTtl = TimeSpan.FromHours(8);

    private CredentialTokenDto _credential;

    protected API? GeotabApi = null;

    public ProtocolType Protocol => ProtocolType.GeoTab;

    // Initializes the Geotab reader with the provided credential, reusing a cached session id
    // when one is live; otherwise authenticates eagerly and caches the new session id.
    public async Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        _credential = credential;

        if (sessionStore.TryGet(credential, out var sessionId))
        {
            GeotabApi = new API(credential.Username, credential.Password, sessionId, credential.Key!);
            return;
        }

        GeotabApi = new API(credential.Username, credential.Password, null, credential.Key!);
        await GeotabApi.AuthenticateAsync(cancellationToken);
        PersistSession();
    }

    // Stores the API client's current session id — call after successful provider calls so a
    // session the SDK silently re-established replaces the stale cached one.
    protected void PersistSession()
    {
        var sessionId = GeotabApi?.SessionId;
        if (!string.IsNullOrEmpty(sessionId))
        {
            sessionStore.Set(_credential, sessionId, SessionTtl);
        }
    }
}
