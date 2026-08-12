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

using Microsoft.EntityFrameworkCore;
using TrackHub.AuthorityServer.Infrastructure;
using TrackHub.AuthorityServer.Infrastructure.Entities;
using TrackHub.AuthorityServer.Infrastructure.Readers;

namespace TrackHub.AuthorityServer.Infrastructure.UnitTests;

// The access token's role claim comes from this lookup over the
// pre-existing security.roles / security.user_role tables (read-only bounded-context mapping).
// The seeded hierarchy is Administrator(1) → Manager(2) → User(3): the lowest id wins.
[TestFixture]
public class UserReaderRoleTests
{
    private static SecurityDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<SecurityDbContext>().UseInMemoryDatabase(name).Options);

    private static async Task SeedRolesAsync(SecurityDbContext context)
    {
        context.Roles.AddRange(
            new Role { RoleId = 1, Name = "Administrator" },
            new Role { RoleId = 2, Name = "Manager" },
            new Role { RoleId = 3, Name = "User" });
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task GetUserRoleAsync_UserWithSingleRole_ReturnsItsName()
    {
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetUserRoleAsync_UserWithSingleRole_ReturnsItsName));
        await SeedRolesAsync(context);
        context.UserRoles.Add(new UserRole { UserId = userId, RoleId = 2 });
        await context.SaveChangesAsync();

        var role = await new UserReader(context).GetUserRoleAsync(userId, CancellationToken.None);

        Assert.That(role, Is.EqualTo("Manager"));
    }

    [Test]
    public async Task GetUserRoleAsync_UserWithMultipleRoles_ReturnsTheMostPrivileged()
    {
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetUserRoleAsync_UserWithMultipleRoles_ReturnsTheMostPrivileged));
        await SeedRolesAsync(context);
        context.UserRoles.Add(new UserRole { UserId = userId, RoleId = 3 });
        context.UserRoles.Add(new UserRole { UserId = userId, RoleId = 1 });
        await context.SaveChangesAsync();

        var role = await new UserReader(context).GetUserRoleAsync(userId, CancellationToken.None);

        Assert.That(role, Is.EqualTo("Administrator"));
    }

    [Test]
    public async Task GetUserRoleAsync_UserWithoutRoleRows_ReturnsNull()
    {
        await using var context = NewContext(nameof(GetUserRoleAsync_UserWithoutRoleRows_ReturnsNull));
        await SeedRolesAsync(context);

        var role = await new UserReader(context).GetUserRoleAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(role, Is.Null);
    }

    [Test]
    public async Task GetUserRoleAsync_OtherUsersRoles_DoNotLeak()
    {
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetUserRoleAsync_OtherUsersRoles_DoNotLeak));
        await SeedRolesAsync(context);
        context.UserRoles.Add(new UserRole { UserId = Guid.NewGuid(), RoleId = 1 });
        context.UserRoles.Add(new UserRole { UserId = userId, RoleId = 3 });
        await context.SaveChangesAsync();

        var role = await new UserReader(context).GetUserRoleAsync(userId, CancellationToken.None);

        Assert.That(role, Is.EqualTo("User"));
    }
}
