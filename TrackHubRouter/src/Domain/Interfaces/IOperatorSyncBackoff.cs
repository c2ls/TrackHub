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

// Per-operator exponential backoff for the background sync/health loops. A persistently failing
// operator (e.g. wrong credentials, a decommissioned provider) must not be re-attempted at full
// cadence forever — that hammers the provider, spams logs, and wastes cycles (router-audit A-15).
// After consecutive failures the operator is held out for an exponentially growing window; the
// first success clears it. In-process, consistent with the single-instance SyncWorker deployment.
public interface IOperatorSyncBackoff
{
    // True when the operator is still within its backoff window and should be skipped this tick.
    bool IsInBackoff(Guid operatorId, DateTimeOffset now);

    // Clears any backoff for the operator (a successful attempt).
    void RecordSuccess(Guid operatorId);

    // Records a failed attempt and extends the backoff window.
    void RecordFailure(Guid operatorId, DateTimeOffset now);
}
