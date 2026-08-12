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
using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using Moq;

namespace Infrastructure.UnitTests;

/// <summary>
/// The picker lookups exist so a consumer fetches a narrow projection instead of draining full
/// pages. Two of them were widened to what their consumers actually render — the POI lookup to the
/// map overlay's colour/type/description/address/active, the transporter lookup to the type the
/// toll-class dialog derives. These tests pin those columns: without them the portal falls back to
/// the full-page drains that were deleted, so a dropped column is a broken screen.
/// </summary>
[TestFixture]
public class LookupProjectionTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        return principal.Object;
    }

    [Test]
    public async Task PointOfInterestLookup_CarriesTheMapOverlayColumns()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(PointOfInterestLookup_CarriesTheMapOverlayColumns));

        var poi = new PointOfInterest("Depot", "loading bay", 2, 10d, 20d, "1 Main St", 3, null, true, accountId);
        await context.PointsOfInterest.AddAsync(poi);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new PointOfInterestReader(context, Principal(accountId));
        var lookup = await reader.GetPointOfInterestLookupAsync(accountId, null, 5_001, CancellationToken.None);

        var row = lookup.Single();
        Assert.Multiple(() =>
        {
            Assert.That(row.Color, Is.EqualTo((short)3));
            Assert.That(row.Type, Is.EqualTo((short)2));
            Assert.That(row.Description, Is.EqualTo("loading bay"));
            Assert.That(row.Address, Is.EqualTo("1 Main St"));
            Assert.That(row.Active, Is.True);
            Assert.That(row.Latitude, Is.EqualTo(10d));
            Assert.That(row.Longitude, Is.EqualTo(20d));
        });
    }

    [Test]
    public async Task TransporterLookupByAccount_CarriesTheTransporterType()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(TransporterLookupByAccount_CarriesTheTransporterType));

        var transporter = new Transporter("Rig", 2, accountId);
        await context.Transporters.AddAsync(transporter);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new TransporterReader(context, Principal(accountId));
        var lookup = await reader.GetTransporterLookupByAccountAsync(accountId, 5_001, CancellationToken.None);

        var row = lookup.Single();
        Assert.Multiple(() =>
        {
            Assert.That(row.TransporterTypeId, Is.EqualTo((short)2));
            Assert.That((short)row.TransporterType, Is.EqualTo((short)2));
        });
    }
}
