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

namespace TrackHub.Router.Infrastructure.Common;

// In-process per-operator mutex (one SemaphoreSlim per operator id). Registered as a singleton so
// the manual-sync and background-sync paths share the same gates (router-audit A-25).
public sealed class OperatorSyncLock : IOperatorSyncLock
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _gates = new();

    public async Task<IDisposable> AcquireAsync(Guid operatorId, CancellationToken cancellationToken)
    {
        var gate = _gates.GetOrAdd(operatorId, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        return new Releaser(gate);
    }

    private sealed class Releaser(SemaphoreSlim gate) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                gate.Release();
            }
        }
    }
}
