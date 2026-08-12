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

// In-process exponential backoff keyed by operator id (router-audit A-15). The window doubles per
// consecutive failure from BaseDelay up to MaxDelay; the first success removes the entry.
public sealed class OperatorSyncBackoff : IOperatorSyncBackoff
{
    private static readonly TimeSpan BaseDelay = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromMinutes(30);

    private readonly ConcurrentDictionary<Guid, State> _state = new();

    public bool IsInBackoff(Guid operatorId, DateTimeOffset now)
        => _state.TryGetValue(operatorId, out var state) && now < state.NextAttemptAt;

    public void RecordSuccess(Guid operatorId)
        => _state.TryRemove(operatorId, out _);

    public void RecordFailure(Guid operatorId, DateTimeOffset now)
        => _state.AddOrUpdate(
            operatorId,
            _ => new State(1, now + BaseDelay),
            (_, existing) =>
            {
                var failures = existing.ConsecutiveFailures + 1;
                return new State(failures, now + DelayFor(failures));
            });

    // 1,2,4,8,16,30,30... minutes (capped). Shift on (failures-1), guarded so the shift never
    // overflows for a long-running poison-pill operator.
    private static TimeSpan DelayFor(int failures)
    {
        var exponent = Math.Min(failures - 1, 5);
        var scaled = TimeSpan.FromTicks(BaseDelay.Ticks << exponent);
        return scaled < MaxDelay ? scaled : MaxDelay;
    }

    private readonly record struct State(int ConsecutiveFailures, DateTimeOffset NextAttemptAt);
}
