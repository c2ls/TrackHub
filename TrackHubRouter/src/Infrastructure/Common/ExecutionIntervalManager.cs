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
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Infrastructure.Common;

public class ExecutionIntervalManager : IExecutionIntervalManager
{
    private readonly ConcurrentDictionary<Guid, DateTimeOffset> _lastExecutionTimes = new();

    public bool ShouldExecuteTask(AccountSettingsVm account)
    {
        if (!_lastExecutionTimes.TryGetValue(account.AccountId, out var lastExecutionTime))
        {
            return true; // If there's no record of the last execution, execute the task
        }

        var interval = TimeSpan.FromSeconds(account.StoringInterval);
        return (DateTimeOffset.UtcNow - lastExecutionTime) >= interval;
    }

    public void UpdateLastExecutionTime(Guid accountId)
    {
        _lastExecutionTimes[accountId] = DateTimeOffset.UtcNow;
    }
}
