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
using Microsoft.EntityFrameworkCore;
using Moq;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;
using DetectedStatus = TrackHub.Manager.Domain.Enums.DetectedStatus;

namespace Infrastructure.UnitTests;

// CreateManualDeviceAsync — the manual-registration path for catalog-less providers (Prosegur).
// InMemory does not enforce the unique (account, operator, identifier) index, so the concurrent
// DbUpdateException path is not reproducible here; the explicit-duplicate pre-check, the identifier
// auto-allocation, provider-metadata nulling and cross-account rejection all are.
[TestFixture]
public class ManualDeviceWriterTests
{
    private const short DeviceTypeId = 4; // Cellular

    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        return principal.Object;
    }

    private static async Task<Guid> SeedOperatorAsync(ApplicationDbContext context, Guid accountId)
    {
        var op = new Operator("Prosegur", null, null, null, null, null, 101, accountId);
        await context.Operators.AddAsync(op);
        await context.SaveChangesAsync();
        return op.OperatorId;
    }

    private static DeviceDto Dto(Guid accountId, Guid operatorId, int identifier = 0, string name = "ABC123") =>
        new(accountId, operatorId, "SER-1", name, identifier, "SHOULD-BE-IGNORED", DeviceTypeId, "desc", "hash", "ACTIVE");

    [Test]
    public async Task CreateManual_AutoAllocatesIdentifierAndDropsProviderMetadata()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateManual_AutoAllocatesIdentifierAndDropsProviderMetadata));
        var operatorId = await SeedOperatorAsync(context, accountId);
        var writer = new DeviceWriter(context, Principal(accountId));

        var vm = await writer.CreateManualDeviceAsync(Dto(accountId, operatorId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Identifier, Is.EqualTo(1), "first device on the operator gets identifier 1");
            Assert.That(vm.DetectedStatus, Is.EqualTo(DetectedStatus.New));
            // Provider metadata belongs to the sync path only; manual devices must not carry it
            // even when a client supplies it.
            Assert.That(vm.ProviderDisplayName, Is.Null);
            Assert.That(vm.ProviderMetadataHash, Is.Null);
            Assert.That(vm.ProviderStatus, Is.Null);
            Assert.That(vm.Name, Is.EqualTo("ABC123"));
        });

        var stored = await context.Devices.SingleAsync();
        Assert.That(stored.ProviderDisplayName, Is.Null);
    }

    [Test]
    public async Task CreateManual_AllocatesNextFreeIdentifierAboveExisting()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateManual_AllocatesNextFreeIdentifierAboveExisting));
        var operatorId = await SeedOperatorAsync(context, accountId);
        await context.Devices.AddAsync(new Device("X", 7, "s", DeviceTypeId, null, null, null, null, (int)DetectedStatus.Available, operatorId, accountId));
        await context.SaveChangesAsync();
        var writer = new DeviceWriter(context, Principal(accountId));

        var vm = await writer.CreateManualDeviceAsync(Dto(accountId, operatorId), CancellationToken.None);

        Assert.That(vm.Identifier, Is.EqualTo(8));
    }

    [Test]
    public async Task CreateManual_ExplicitDuplicateIdentifier_ThrowsConflict()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateManual_ExplicitDuplicateIdentifier_ThrowsConflict));
        var operatorId = await SeedOperatorAsync(context, accountId);
        await context.Devices.AddAsync(new Device("X", 5, "s", DeviceTypeId, null, null, null, null, (int)DetectedStatus.Available, operatorId, accountId));
        await context.SaveChangesAsync();
        var writer = new DeviceWriter(context, Principal(accountId));

        Assert.ThrowsAsync<ConflictException>(() =>
            writer.CreateManualDeviceAsync(Dto(accountId, operatorId, identifier: 5), CancellationToken.None));
    }

    [Test]
    public async Task CreateManual_OperatorInAnotherAccount_ThrowsForbidden()
    {
        var accountId = Guid.NewGuid();
        var otherAccountId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateManual_OperatorInAnotherAccount_ThrowsForbidden));
        // Operator belongs to a different account than the caller/DTO claims.
        var operatorId = await SeedOperatorAsync(context, otherAccountId);
        var writer = new DeviceWriter(context, Principal(accountId));

        Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            writer.CreateManualDeviceAsync(Dto(accountId, operatorId), CancellationToken.None));
    }
}
