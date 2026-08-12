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

using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using TrackHub.Security.Domain.Records;
using TrackHub.Security.Infrastructure;
using TrackHub.Security.Infrastructure.Entities;
using TrackHub.Security.Infrastructure.Readers;
using TrackHub.Security.Infrastructure.Writers;

namespace Infrastructure.UnitTests;

/// <summary>
/// Foreign-deny pins for the TS-06 by-id guards (the enforcement the
/// <c>[AccountScopeEnforcedInHandler]</c> markers on the user requests cite): a caller from one
/// account must not read, mutate, delete, or grant roles/policies against a user owned by another
/// account, while same-account callers, the Administrator, and global service identities keep
/// working. Removing a RequireAccountAccess call fails one of these.
/// </summary>
[TestFixture]
internal class AccountScopeGuardTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static User NewUser(Guid accountId, string username)
        => new(username, "pw", $"{username}@mail.com", "first", null, "last", null, null, true, 0, accountId);

    private static ICurrentPrincipal Principal(PrincipalType type, Guid? accountId, string? role = null)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(type);
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.Role).Returns(role);
        principal.SetupGet(p => p.UserId).Returns(Guid.NewGuid());
        return principal.Object;
    }

    private static ICurrentPrincipal ForeignUser() => Principal(PrincipalType.User, Guid.NewGuid(), Roles.Manager);

    private static UpdateUserDto UpdateDtoFor(User user)
        => new(user.UserId, user.Username, user.EmailAddress, user.FirstName, user.SecondName, user.LastName, user.SecondSurname, user.DOB, true, false);

    [Test]
    public async Task Reader_GetUser_ForeignAccount_IsForbidden()
    {
        await using var context = NewContext(nameof(Reader_GetUser_ForeignAccount_IsForbidden));
        var target = NewUser(Guid.NewGuid(), "target");
        await context.Users.AddAsync(target);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new UserReader(context, ForeignUser());

        Assert.ThrowsAsync<ForbiddenAccessException>(() => reader.GetUserAsync(target.UserId, CancellationToken.None));
    }

    [Test]
    public async Task Reader_GetUser_SameAccount_Passes()
    {
        await using var context = NewContext(nameof(Reader_GetUser_SameAccount_Passes));
        var target = NewUser(Guid.NewGuid(), "target");
        await context.Users.AddAsync(target);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new UserReader(context, Principal(PrincipalType.User, target.AccountId, Roles.Manager));
        var vm = await reader.GetUserAsync(target.UserId, CancellationToken.None);

        Assert.That(vm.UserId, Is.EqualTo(target.UserId));
    }

    [Test]
    public async Task Reader_GetUser_Administrator_ReadsAnyAccount()
    {
        await using var context = NewContext(nameof(Reader_GetUser_Administrator_ReadsAnyAccount));
        var target = NewUser(Guid.NewGuid(), "target");
        await context.Users.AddAsync(target);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new UserReader(context, Principal(PrincipalType.User, Guid.NewGuid(), Roles.Administrator));
        var vm = await reader.GetUserAsync(target.UserId, CancellationToken.None);

        Assert.That(vm.UserId, Is.EqualTo(target.UserId));
    }

    [Test]
    public async Task Writer_UpdateUser_ForeignAccount_IsForbidden_AndWritesNothing()
    {
        await using var context = NewContext(nameof(Writer_UpdateUser_ForeignAccount_IsForbidden_AndWritesNothing));
        var target = NewUser(Guid.NewGuid(), "target");
        await context.Users.AddAsync(target);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new UserWriter(context, ForeignUser());

        Assert.ThrowsAsync<ForbiddenAccessException>(() => writer.UpdateUserAsync(
            UpdateDtoFor(target) with { Username = "hijacked" }, CancellationToken.None));
        Assert.That((await context.Users.FindAsync(target.UserId))!.Username, Is.EqualTo("target"));
    }

    [Test]
    public async Task Writer_UpdatePassword_ForeignAccount_IsForbidden()
    {
        await using var context = NewContext(nameof(Writer_UpdatePassword_ForeignAccount_IsForbidden));
        var target = NewUser(Guid.NewGuid(), "target");
        await context.Users.AddAsync(target);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new UserWriter(context, ForeignUser());

        Assert.ThrowsAsync<ForbiddenAccessException>(() => writer.UpdatePasswordAsync(
            new UserPasswordDto(target.UserId, "newPassword1!"), CancellationToken.None));
    }

    [Test]
    public async Task Writer_UnlockUser_ForeignAccount_IsForbidden()
    {
        await using var context = NewContext(nameof(Writer_UnlockUser_ForeignAccount_IsForbidden));
        var target = NewUser(Guid.NewGuid(), "target");
        await context.Users.AddAsync(target);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new UserWriter(context, ForeignUser());

        Assert.ThrowsAsync<ForbiddenAccessException>(() => writer.UnlockUserAsync(target.UserId, CancellationToken.None));
    }

    [Test]
    public async Task Writer_DeleteUser_ForeignAccount_IsForbidden_AndRowSurvives()
    {
        await using var context = NewContext(nameof(Writer_DeleteUser_ForeignAccount_IsForbidden_AndRowSurvives));
        var target = NewUser(Guid.NewGuid(), "target");
        await context.Users.AddAsync(target);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new UserWriter(context, ForeignUser());

        Assert.ThrowsAsync<ForbiddenAccessException>(() => writer.DeleteUserAsync(target.UserId, CancellationToken.None));
        Assert.That(await context.Users.FindAsync(target.UserId), Is.Not.Null);
    }

    [Test]
    public async Task Writer_DeleteUser_SameAccount_Deletes()
    {
        await using var context = NewContext(nameof(Writer_DeleteUser_SameAccount_Deletes));
        var target = NewUser(Guid.NewGuid(), "target");
        await context.Users.AddAsync(target);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new UserWriter(context, Principal(PrincipalType.User, target.AccountId, Roles.Manager));
        await writer.DeleteUserAsync(target.UserId, CancellationToken.None);

        Assert.That(await context.Users.FindAsync(target.UserId), Is.Null);
    }

    [Test]
    public async Task Writer_CreateUser_ForeignAccount_IsForbidden()
    {
        await using var context = NewContext(nameof(Writer_CreateUser_ForeignAccount_IsForbidden));
        var writer = new UserWriter(context, ForeignUser());
        var dto = new CreateUserDto("new-user", "pw", "new@mail.com", "first", null, "last", null, null, true, false);

        Assert.ThrowsAsync<ForbiddenAccessException>(() => writer.CreateUserAsync(dto, Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public async Task RoleWriter_Grant_ForeignAccountTarget_IsForbidden()
    {
        await using var context = NewContext(nameof(RoleWriter_Grant_ForeignAccountTarget_IsForbidden));
        var target = NewUser(Guid.NewGuid(), "target");
        await context.Users.AddAsync(target);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new UserRoleWriter(context, ForeignUser());

        Assert.ThrowsAsync<ForbiddenAccessException>(() => writer.CreateUserRoleAsync(
            new UserRoleDto(target.UserId, 1), CancellationToken.None));
    }

    [Test]
    public async Task PolicyWriter_Grant_ForeignAccountTarget_IsForbidden()
    {
        await using var context = NewContext(nameof(PolicyWriter_Grant_ForeignAccountTarget_IsForbidden));
        var target = NewUser(Guid.NewGuid(), "target");
        await context.Users.AddAsync(target);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new UserPolicyWriter(context, ForeignUser());

        Assert.ThrowsAsync<ForbiddenAccessException>(() => writer.CreateUserPolicyAsync(
            new UserPolicyDto(target.UserId, 1), CancellationToken.None));
    }
}
