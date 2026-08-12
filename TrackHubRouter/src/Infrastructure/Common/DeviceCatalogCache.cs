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
using Microsoft.Extensions.Configuration;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Infrastructure.Common;

// In-process TTL cache of operator device catalogs (router-audit A-12). Consistent with the
// single-instance SyncWorker deployment; TTL bounds staleness for changes made outside the Router
// (e.g. transporter re-assignment in Manager) while device-sync invalidation covers changes the
// Router itself makes.
public sealed class DeviceCatalogCache : IDeviceCatalogCache
{
    private readonly TimeSpan _ttl;
    private readonly ConcurrentDictionary<Guid, Entry> _entries = new();

    public DeviceCatalogCache(IConfiguration configuration)
    {
        _ttl = int.TryParse(configuration["AppSettings:DeviceCatalogCacheSeconds"], out var seconds) && seconds >= 0
            ? TimeSpan.FromSeconds(seconds)
            : TimeSpan.FromSeconds(60);
    }

    public async Task<IEnumerable<DeviceTransporterVm>> GetOrLoadAsync(
        Guid accountId,
        Guid operatorId,
        Func<CancellationToken, Task<IEnumerable<DeviceTransporterVm>>> loader,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        if (_ttl > TimeSpan.Zero
            && _entries.TryGetValue(operatorId, out var entry)
            && entry.AccountId == accountId
            && now < entry.ExpiresAt)
        {
            return entry.Devices;
        }

        // Cache miss / expired / disabled: load fresh. A rare concurrent stampede just loads twice —
        // harmless (idempotent read), so no per-key lock is taken.
        var devices = (await loader(cancellationToken))?.ToArray() ?? [];

        if (_ttl > TimeSpan.Zero)
        {
            _entries[operatorId] = new Entry(accountId, devices, now + _ttl);
        }

        return devices;
    }

    public void Invalidate(Guid operatorId)
        => _entries.TryRemove(operatorId, out _);

    private readonly record struct Entry(Guid AccountId, IReadOnlyList<DeviceTransporterVm> Devices, DateTimeOffset ExpiresAt);
}
