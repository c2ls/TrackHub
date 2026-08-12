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

// Serializes device synchronization per operator within a process so a user's manual sync and the
// background sync loop (or two manual triggers) cannot run concurrently for the same operator —
// which would race the device-catalog reset/rebuild and duplicate work (router-audit A-25). This
// is an in-process guard consistent with the single-instance SyncWorker deployment; a
// cross-instance claim (advisory lock / SKIP LOCKED) would be required to scale the worker out.
public interface IOperatorSyncLock
{
    // Acquires the per-operator gate; dispose the returned handle to release it.
    Task<IDisposable> AcquireAsync(Guid operatorId, CancellationToken cancellationToken);
}
