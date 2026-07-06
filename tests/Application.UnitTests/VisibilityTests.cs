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
using Common.Domain.Constants;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Entities;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Readers;

namespace TrackHub.Telemetry.Application.UnitTests;

[TestFixture]
public class VisibilityTests
{
    // Account A has two transporters; only one is in the plain user's group.
    private static (Infrastructure.TelemetryDB.ApplicationDbContext Context, Guid AccountId, Guid UserId, Guid InGroup, Guid OutOfGroup) Seed()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var inGroup = Guid.NewGuid();
        var outOfGroup = Guid.NewGuid();
        const long groupId = 1L;
        var context = TestDb.NewContext();
        context.Transporters.Add(new Transporter { TransporterId = inGroup, AccountId = accountId, Name = "In", TransporterTypeId = 1 });
        context.Transporters.Add(new Transporter { TransporterId = outOfGroup, AccountId = accountId, Name = "Out", TransporterTypeId = 1 });
        context.Groups.Add(new Group { GroupId = groupId, AccountId = accountId });
        context.Set<TransporterGroup>().Add(new TransporterGroup { TransporterId = inGroup, GroupId = groupId });
        context.UsersGroup.Add(new UserGroup { UserId = userId, GroupId = groupId });
        context.SaveChanges();
        return (context, accountId, userId, inGroup, outOfGroup);
    }

    [Test]
    public async Task PlainUser_SeesOnlyTransportersInTheirGroups()
    {
        var (context, accountId, userId, inGroup, outOfGroup) = Seed();
        var principal = TestDb.PrincipalFor(accountId, PrincipalType.User, userId, role: null);
        var reader = new VisibleTransporterReader(context, principal);

        var visible = await reader.GetVisibleTransporterIdsAsync(userId, accountId, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(visible, Does.Contain(inGroup));
            Assert.That(visible, Does.Not.Contain(outOfGroup), "a plain user cannot see a transporter outside their groups");
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task PrivilegedRole_SeesEveryTransporterInTheAccount()
    {
        var (context, accountId, userId, inGroup, outOfGroup) = Seed();
        var principal = TestDb.PrincipalFor(accountId, PrincipalType.User, userId, role: Roles.Manager);
        var reader = new VisibleTransporterReader(context, principal);

        var visible = await reader.GetVisibleTransporterIdsAsync(userId, accountId, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(visible, Does.Contain(inGroup));
            Assert.That(visible, Does.Contain(outOfGroup), "Administrator/Manager reads account-wide, regardless of group membership");
        }
        await context.DisposeAsync();
    }
}
