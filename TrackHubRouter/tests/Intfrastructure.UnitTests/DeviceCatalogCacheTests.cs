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

using Microsoft.Extensions.Configuration;
using Moq;
using TrackHub.Router.Domain.Models;
using TrackHub.Router.Infrastructure.Common;

namespace TrackHub.Router.Infrastructure.Tests;

// Guards the device-catalog cache (router-audit A-12).
[TestFixture]
public class DeviceCatalogCacheTests
{
    private static readonly Guid AccountId = Guid.NewGuid();
    private static readonly Guid OperatorId = Guid.NewGuid();

    private static DeviceCatalogCache CacheWithTtl(string? seconds)
    {
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["AppSettings:DeviceCatalogCacheSeconds"]).Returns(seconds);
        return new DeviceCatalogCache(config.Object);
    }

    private static Task<IEnumerable<DeviceTransporterVm>> LoadOne()
        => Task.FromResult<IEnumerable<DeviceTransporterVm>>([new DeviceTransporterVm { TransporterId = Guid.NewGuid() }]);

    [Test]
    public async Task GetOrLoad_CachesWithinTtl_LoadsOnce()
    {
        var cache = CacheWithTtl("60");
        var loads = 0;

        for (var i = 0; i < 3; i++)
        {
            await cache.GetOrLoadAsync(AccountId, OperatorId, _ => { loads++; return LoadOne(); }, CancellationToken.None);
        }

        Assert.That(loads, Is.EqualTo(1), "subsequent reads within the TTL must be served from cache");
    }

    [Test]
    public async Task Invalidate_ForcesReload()
    {
        var cache = CacheWithTtl("60");
        var loads = 0;

        await cache.GetOrLoadAsync(AccountId, OperatorId, _ => { loads++; return LoadOne(); }, CancellationToken.None);
        cache.Invalidate(OperatorId);
        await cache.GetOrLoadAsync(AccountId, OperatorId, _ => { loads++; return LoadOne(); }, CancellationToken.None);

        Assert.That(loads, Is.EqualTo(2), "invalidation must force a fresh load");
    }

    [Test]
    public async Task ZeroTtl_DisablesCaching()
    {
        var cache = CacheWithTtl("0");
        var loads = 0;

        await cache.GetOrLoadAsync(AccountId, OperatorId, _ => { loads++; return LoadOne(); }, CancellationToken.None);
        await cache.GetOrLoadAsync(AccountId, OperatorId, _ => { loads++; return LoadOne(); }, CancellationToken.None);

        Assert.That(loads, Is.EqualTo(2), "a zero TTL disables caching (always loads)");
    }
}
