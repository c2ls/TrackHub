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

using Common.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using TrackHub.Security.Infrastructure;
using TrackHub.Security.Infrastructure.Entities;
using TrackHub.Security.Infrastructure.Interfaces;
using TrackHub.Security.Infrastructure.Readers;

namespace Infrastructure.UnitTests;

// Driver device read models mask the push token (trailing fragment only) and expose
// no RefreshTokenFamilyId (absent from the VM entirely). The list is paged with stable ordering.
[TestFixture]
public class DriverIdentityReaderTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal AccountPrincipal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        return principal.Object;
    }

    private static DriverDeviceRegistration Device(Guid accountId, string pushToken, DateTimeOffset lastSeenAt)
        => new(Guid.NewGuid(), accountId, "device", "Phone", "android", "1.0", pushToken, "family-secret")
        {
            LastSeenAt = lastSeenAt
        };

    [Test]
    public async Task GetDriverDevices_MasksPushToken()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetDriverDevices_MasksPushToken));
        await context.DriverDeviceRegistrations.AddAsync(Device(accountId, "abcdef1234567890", DateTimeOffset.UtcNow));
        await context.SaveChangesAsync(CancellationToken.None);
        var reader = new DriverIdentityReader(context as IApplicationDbContext, AccountPrincipal(accountId));

        var device = (await reader.GetDriverDevicesAsync(accountId, null, 0, 50, CancellationToken.None)).Single();

        Assert.That(device.PushToken, Is.EqualTo("******567890"), "only the trailing 6 chars are exposed");
        Assert.That(device.PushToken, Does.Not.Contain("abcdef"), "the leading push-token material is masked");
    }

    [Test]
    public async Task GetDriverDevices_PagesWithStableDescendingOrder()
    {
        var accountId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        await using var context = NewContext(nameof(GetDriverDevices_PagesWithStableDescendingOrder));
        await context.DriverDeviceRegistrations.AddAsync(Device(accountId, "token-oldest", now.AddHours(-2)));
        await context.DriverDeviceRegistrations.AddAsync(Device(accountId, "token-newest", now));
        await context.DriverDeviceRegistrations.AddAsync(Device(accountId, "token-middle", now.AddHours(-1)));
        await context.SaveChangesAsync(CancellationToken.None);
        var reader = new DriverIdentityReader(context as IApplicationDbContext, AccountPrincipal(accountId));

        var page1 = await reader.GetDriverDevicesAsync(accountId, null, 0, 2, CancellationToken.None);
        var page2 = await reader.GetDriverDevicesAsync(accountId, null, 2, 2, CancellationToken.None);

        // Ordered by LastSeenAt descending: newest, middle, oldest.
        Assert.That(page1, Has.Count.EqualTo(2));
        Assert.That(page1.First().PushToken, Is.EqualTo("******newest"));
        Assert.That(page2, Has.Count.EqualTo(1), "the second page holds the remaining device");
    }
}
