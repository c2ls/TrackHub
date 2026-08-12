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

using TrackHub.Manager.Application.GpsIntegration.Queries;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.GpsIntegration;

[TestFixture]
public class GetSynchronizedDevicesQueryHandlerTests
{
    private static SynchronizedDeviceFilter Capture(GetSynchronizedDevicesQuery query)
    {
        var reader = new Mock<IDeviceReader>();
        SynchronizedDeviceFilter captured = default;
        reader
            .Setup(r => r.SearchDevicesAsync(
                It.IsAny<Guid>(),
                It.IsAny<SynchronizedDeviceFilter>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, SynchronizedDeviceFilter, int, int, string?, CancellationToken>(
                (_, filter, _, _, _, _) => captured = filter)
            .ReturnsAsync(new DevicesPageVm([], 0));

        var handler = new GetSynchronizedDevicesQueryHandler(reader.Object);
        handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();
        return captured;
    }

    [Test]
    public void Handle_RecentOnly_SetsFirstSeenSinceInsideTheWindow()
    {
        var before = DateTimeOffset.UtcNow - GetSynchronizedDevicesQueryHandler.RecentWindow;

        var filter = Capture(new GetSynchronizedDevicesQuery(Guid.NewGuid(), RecentOnly: true));

        var after = DateTimeOffset.UtcNow - GetSynchronizedDevicesQueryHandler.RecentWindow;
        Assert.That(filter.FirstSeenSince, Is.Not.Null);
        // The cutoff must be ~24h ago, not "now" and not null — a mutant that passed
        // DateTimeOffset.UtcNow (no window) or left it null would surface here.
        Assert.That(filter.FirstSeenSince!.Value, Is.InRange(before, after));
    }

    [Test]
    public void Handle_RecentOnlyFalse_LeavesFirstSeenSinceNull()
    {
        var filter = Capture(new GetSynchronizedDevicesQuery(Guid.NewGuid(), RecentOnly: false));

        Assert.That(filter.FirstSeenSince, Is.Null);
    }

    [Test]
    public void Handle_ForwardsUnassignedOnlyVerbatim()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Capture(new GetSynchronizedDevicesQuery(Guid.NewGuid(), UnassignedOnly: true)).UnassignedOnly, Is.True);
            Assert.That(Capture(new GetSynchronizedDevicesQuery(Guid.NewGuid(), UnassignedOnly: false)).UnassignedOnly, Is.False);
        });
    }
}
