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
using Moq;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

/// <summary>
/// Foreign-deny pins for the TS-06 by-id guards added to the previously unguarded Manager
/// writers/readers: OperatorWriter, TransporterWriter, the user REPLICA writer (Security-parity
/// policy), UserSettingsWriter (self-bind) and the by-id account reads. Removing a guard call
/// fails one of these.
/// </summary>
[TestFixture]
public class AccountScopeGuardTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(PrincipalType type, Guid? accountId, string? role = null, Guid? userId = null)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(type);
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.Role).Returns(role);
        principal.SetupGet(p => p.UserId).Returns(userId ?? Guid.NewGuid());
        return principal.Object;
    }

    private static ICurrentPrincipal ForeignUser() => Principal(PrincipalType.User, Guid.NewGuid(), Roles.Manager);

    [Test]
    public async Task OperatorWriter_Update_ForeignAccount_IsForbidden()
    {
        await using var context = NewContext(nameof(OperatorWriter_Update_ForeignAccount_IsForbidden));
        var op = new Operator("gps", null, null, null, null, null, 1, Guid.NewGuid());
        await context.Operators.AddAsync(op);
        await context.SaveChangesAsync();

        var writer = new OperatorWriter(context as IApplicationDbContext, ForeignUser());

        Assert.ThrowsAsync<ForbiddenAccessException>(() => writer.SetEnabledAsync(op.OperatorId, true, CancellationToken.None));
    }

    [Test]
    public async Task TransporterWriter_UpdateAndDelete_ForeignAccount_IsForbidden()
    {
        await using var context = NewContext(nameof(TransporterWriter_UpdateAndDelete_ForeignAccount_IsForbidden));
        var transporter = new Transporter("truck", 1, Guid.NewGuid());
        await context.Transporters.AddAsync(transporter);
        await context.SaveChangesAsync();

        var writer = new TransporterWriter(context as IApplicationDbContext, ForeignUser());

        Assert.ThrowsAsync<ForbiddenAccessException>(() => writer.UpdateTransporterAsync(
            new UpdateTransporterDto(transporter.TransporterId, "hijacked", 1), CancellationToken.None));
        Assert.ThrowsAsync<ForbiddenAccessException>(() => writer.DeleteTransporterAsync(
            transporter.TransporterId, CancellationToken.None));
        Assert.That(await context.Transporters.FindAsync(transporter.TransporterId), Is.Not.Null);
    }

    [Test]
    public async Task TransporterWriter_SameAccount_Updates()
    {
        await using var context = NewContext(nameof(TransporterWriter_SameAccount_Updates));
        var transporter = new Transporter("truck", 1, Guid.NewGuid());
        await context.Transporters.AddAsync(transporter);
        await context.SaveChangesAsync();

        var writer = new TransporterWriter(
            context as IApplicationDbContext, Principal(PrincipalType.User, transporter.AccountId, Roles.Manager));
        await writer.UpdateTransporterAsync(
            new UpdateTransporterDto(transporter.TransporterId, "renamed", 1), CancellationToken.None);

        Assert.That((await context.Transporters.FindAsync(transporter.TransporterId))!.Name, Is.EqualTo("renamed"));
    }

    [Test]
    public async Task UserReplicaWriter_ForeignManager_CannotTamper_ButAdministratorAndServiceCan()
    {
        await using var context = NewContext(nameof(UserReplicaWriter_ForeignManager_CannotTamper_ButAdministratorAndServiceCan));
        var user = new User(Guid.NewGuid(), "victim", true, Guid.NewGuid());
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var foreign = new UserWriter(context as IApplicationDbContext, ForeignUser());
        Assert.ThrowsAsync<ForbiddenAccessException>(() => foreign.DeleteUserAsync(user.UserId, CancellationToken.None));
        Assert.ThrowsAsync<ForbiddenAccessException>(() => foreign.CreateUserAsync(
            new UserDto(Guid.NewGuid(), "intruder", true, user.AccountId), CancellationToken.None));

        // Administrator parity with Security (tenant onboarding/maintenance relays under the admin token).
        var admin = new UserWriter(
            context as IApplicationDbContext, Principal(PrincipalType.User, Guid.NewGuid(), Roles.Administrator));
        await admin.UpdateUserAsync(new UpdateUserDto(user.UserId, "renamed", true), CancellationToken.None);
        Assert.That((await context.Users.FindAsync(user.UserId))!.Username, Is.EqualTo("renamed"));

        var service = new UserWriter(
            context as IApplicationDbContext, Principal(PrincipalType.ServiceClient, null));
        await service.DeleteUserAsync(user.UserId, CancellationToken.None);
        Assert.That(await context.Users.FindAsync(user.UserId), Is.Null);
    }

    [Test]
    public async Task UserSettingsWriter_Update_ForForeignUser_IsForbidden_ButSelfPasses()
    {
        await using var context = NewContext(nameof(UserSettingsWriter_Update_ForForeignUser_IsForbidden_ButSelfPasses));
        var ownerId = Guid.NewGuid();
        await context.UserSettings.AddAsync(new UserSettings(ownerId));
        await context.SaveChangesAsync();

        var foreign = new UserSettingsWriter(
            context as IApplicationDbContext, Principal(PrincipalType.User, Guid.NewGuid(), Roles.User));
        Assert.ThrowsAsync<ForbiddenAccessException>(() => foreign.UpdateUserSettingsAsync(
            new UserSettingsDto("es", "dark", "compact", ownerId), CancellationToken.None));

        var self = new UserSettingsWriter(
            context as IApplicationDbContext, Principal(PrincipalType.User, Guid.NewGuid(), Roles.User, ownerId));
        await self.UpdateUserSettingsAsync(new UserSettingsDto("es", "dark", "compact", ownerId), CancellationToken.None);
        Assert.That((await context.UserSettings.FindAsync(ownerId))!.Language, Is.EqualTo("es"));
    }

    [Test]
    public async Task AccountReader_ForeignAccount_IsForbidden()
    {
        await using var context = NewContext(nameof(AccountReader_ForeignAccount_IsForbidden));
        var account = new Account("tenant", "tenant account", 1, true);
        await context.Accounts.AddAsync(account);
        await context.SaveChangesAsync();

        var reader = new AccountReader(context as IApplicationDbContext, ForeignUser());

        Assert.ThrowsAsync<ForbiddenAccessException>(() => reader.GetAccountAsync(account.AccountId, CancellationToken.None));
    }
}
