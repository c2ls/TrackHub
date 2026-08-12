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

using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Domain.Interfaces;

// Short-TTL cache of an operator's device→transporter catalog, keyed by (account, operator). The
// 10-second position loop was re-fetching this rarely-changing catalog from Manager on every cycle
// per operator (router-audit A-12); the cache collapses that to one load per TTL, and the
// device-sync loop invalidates the entry whenever it changes the catalog. Keyed by the explicit
// account+operator scope (never a caller identity), so it is safe to cache.
public interface IDeviceCatalogCache
{
    Task<IEnumerable<DeviceTransporterVm>> GetOrLoadAsync(
        Guid accountId,
        Guid operatorId,
        Func<CancellationToken, Task<IEnumerable<DeviceTransporterVm>>> loader,
        CancellationToken cancellationToken);

    // Drops the cached catalog for an operator (called after a device sync mutates it).
    void Invalidate(Guid operatorId);
}
