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
using TrackHub.Router.Domain.Exceptions;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Domain.Helpers;

/// <summary>
/// How many per-device calls a provider with no bulk endpoint may have in flight at once.
/// <para>
/// The position loop runs every 10 seconds. Fetching 200 vehicles one at a time at ~200 ms each is
/// ~40 s — four cycles — so the operator's slot in the global sync semaphore stayed permanently
/// occupied and positions arrived minutes stale. Deliberately modest: the point is to fit the
/// cycle, not to flood a provider that has no batch API for a reason.
/// </para>
/// </summary>
public static class ProviderConcurrency
{
    public const int MaxConcurrentDeviceReads = 6;

    // Failures before anything has been read mean the provider is down: the rest is not attempted,
    // so an outage costs a few timeouts instead of one per device.
    public const int FailuresThatMeanTheProviderIsDown = 3;

    public static async Task<IReadOnlyCollection<PositionVm>> ReadEachDeviceAsync(
        IEnumerable<DeviceTransporterVm> devices,
        Func<DeviceTransporterVm, CancellationToken, Task<PositionVm>> read,
        CancellationToken cancellationToken)
    {
        var batch = devices.ToArray();
        var positions = new ConcurrentBag<PositionVm>();
        Exception? firstFailure = null;
        var succeeded = 0;
        var failed = 0;
        using var giveUp = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        try
        {
            await Parallel.ForEachAsync(
                batch,
                new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrentDeviceReads, CancellationToken = giveUp.Token },
                async (device, token) =>
                {
                    try
                    {
                        var position = await read(device, token);
                        if (position.TransporterId != Guid.Empty)
                        {
                            positions.Add(position);
                        }
                        Interlocked.Increment(ref succeeded);
                    }
                    catch (Exception ex) when (!token.IsCancellationRequested)
                    {
                        Interlocked.CompareExchange(ref firstFailure, ex, null);
                        if (Interlocked.Increment(ref failed) >= FailuresThatMeanTheProviderIsDown && Volatile.Read(ref succeeded) == 0)
                        {
                            giveUp.Cancel();
                        }
                    }
                });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
        }

        IReadOnlyCollection<PositionVm> distinct = [.. positions.Distinct()];
        return firstFailure is null
            ? distinct
            : throw new PartialPositionReadException(distinct, batch.Length - succeeded, batch.Length, firstFailure);
    }
}
