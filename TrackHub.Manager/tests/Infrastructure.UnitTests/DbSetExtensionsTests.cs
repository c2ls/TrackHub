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

using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Infrastructure;

namespace Infrastructure.UnitTests;

[TestFixture]
public class DbSetExtensionsTests
{
    [Test]
    public async Task BulkAddOrUpdateAsync_AddsNewEntities_WhenNoneExist()
    {
        // Arrange
        var dbName = nameof(BulkAddOrUpdateAsync_AddsNewEntities_WhenNoneExist);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using var context = new ApplicationDbContext(options);

        var transporterId1 = Guid.NewGuid();
        var transporterId2 = Guid.NewGuid();

        var positions = new List<TransporterPosition>
        {
            CreatePosition(transporterId1, 10.0, 20.0),
            CreatePosition(transporterId2, 30.0, 40.0)
        };

        // Act
        await context.TransporterPositions.BulkAddOrUpdateAsync(
            positions,
            p => p.TransporterId,
            ["TransporterPositionId"],
            CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await context.TransporterPositions.ToListAsync();
        Assert.That(result, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Any(p => p.TransporterId == transporterId1 && p.Latitude == 10.0), Is.True);
            Assert.That(result.Any(p => p.TransporterId == transporterId2 && p.Latitude == 30.0), Is.True);
        }
    }

    [Test]
    public async Task BulkAddOrUpdateAsync_UpdatesExistingEntities_WhenTheyExist()
    {
        // Arrange
        var dbName = nameof(BulkAddOrUpdateAsync_UpdatesExistingEntities_WhenTheyExist);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var transporterId = Guid.NewGuid();

        await using var context = new ApplicationDbContext(options);

        // Add existing position
        var existingPosition = CreatePosition(transporterId, 10.0, 20.0);
        await context.TransporterPositions.AddAsync(existingPosition);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create updated position
        var updatedPositions = new List<TransporterPosition>
        {
            CreatePosition(transporterId, 50.0, 60.0, speed: 100.0)
        };

        // Act
        await context.TransporterPositions.BulkAddOrUpdateAsync(
            updatedPositions,
            p => p.TransporterId,
            ["TransporterPositionId"],
            CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await context.TransporterPositions.ToListAsync();
        Assert.That(result, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].Latitude, Is.EqualTo(50.0));
            Assert.That(result[0].Longitude, Is.EqualTo(60.0));
            Assert.That(result[0].Speed, Is.EqualTo(100.0));
        }
    }

    [Test]
    public async Task BulkAddOrUpdateAsync_MixedAddAndUpdate_HandlesCorrectly()
    {
        // Arrange
        var dbName = nameof(BulkAddOrUpdateAsync_MixedAddAndUpdate_HandlesCorrectly);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var existingTransporterId = Guid.NewGuid();
        var newTransporterId = Guid.NewGuid();

        await using var context = new ApplicationDbContext(options);

        // Add existing position
        var existingPosition = CreatePosition(existingTransporterId, 10.0, 20.0);
        await context.TransporterPositions.AddAsync(existingPosition);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create batch with one update and one new
        var positions = new List<TransporterPosition>
        {
            CreatePosition(existingTransporterId, 99.0, 99.0), // Update
            CreatePosition(newTransporterId, 11.0, 22.0)       // New
        };

        // Act
        await context.TransporterPositions.BulkAddOrUpdateAsync(
            positions,
            p => p.TransporterId,
            ["TransporterPositionId"],
            CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await context.TransporterPositions.ToListAsync();
        Assert.That(result, Has.Count.EqualTo(2));

        var updated = result.First(p => p.TransporterId == existingTransporterId);
        Assert.That(updated.Latitude, Is.EqualTo(99.0));

        var added = result.First(p => p.TransporterId == newTransporterId);
        Assert.That(added.Latitude, Is.EqualTo(11.0));
    }

    [Test]
    public async Task BulkAddOrUpdateAsync_ExcludesSpecifiedProperties_FromUpdate()
    {
        // Arrange
        var dbName = nameof(BulkAddOrUpdateAsync_ExcludesSpecifiedProperties_FromUpdate);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var transporterId = Guid.NewGuid();
        var originalPositionId = Guid.NewGuid();

        await using var context = new ApplicationDbContext(options);

        // Add existing position with a specific TransporterPositionId
        var existingPosition = CreatePosition(transporterId, 10.0, 20.0);
        existingPosition.TransporterPositionId = originalPositionId;
        await context.TransporterPositions.AddAsync(existingPosition);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create updated position with different TransporterPositionId
        var updatedPosition = CreatePosition(transporterId, 50.0, 60.0);
        updatedPosition.TransporterPositionId = Guid.NewGuid(); // Different ID

        // Act
        await context.TransporterPositions.BulkAddOrUpdateAsync(
            [updatedPosition],
            p => p.TransporterId,
            ["TransporterPositionId"], // Exclude this property
            CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await context.TransporterPositions.FirstAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TransporterPositionId, Is.EqualTo(originalPositionId)); // Should not change
            Assert.That(result.Latitude, Is.EqualTo(50.0)); // Should change
        }
    }

    [Test]
    public async Task BulkAddOrUpdateAsync_HandlesDuplicateKeys_KeepsLastOccurrence()
    {
        // Arrange
        var dbName = nameof(BulkAddOrUpdateAsync_HandlesDuplicateKeys_KeepsLastOccurrence);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var transporterId = Guid.NewGuid();

        await using var context = new ApplicationDbContext(options);

        // Create multiple positions for the same transporter
        var positions = new List<TransporterPosition>
        {
            CreatePosition(transporterId, 10.0, 20.0, speed: 50.0),   // First
            CreatePosition(transporterId, 30.0, 40.0, speed: 75.0),   // Second
            CreatePosition(transporterId, 50.0, 60.0, speed: 100.0)   // Last - should be kept
        };

        // Act
        await context.TransporterPositions.BulkAddOrUpdateAsync(
            positions,
            p => p.TransporterId,
            ["TransporterPositionId"],
            CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await context.TransporterPositions.ToListAsync();
        Assert.That(result, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].Latitude, Is.EqualTo(50.0));
            Assert.That(result[0].Speed, Is.EqualTo(100.0));
        }
    }

    [Test]
    public async Task BulkAddOrUpdateAsync_HandlesDuplicateKeys_UpdatesWithLastOccurrence()
    {
        // Arrange
        var dbName = nameof(BulkAddOrUpdateAsync_HandlesDuplicateKeys_UpdatesWithLastOccurrence);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var transporterId = Guid.NewGuid();

        await using var context = new ApplicationDbContext(options);

        // Add existing position
        var existingPosition = CreatePosition(transporterId, 1.0, 2.0, speed: 10.0);
        await context.TransporterPositions.AddAsync(existingPosition);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create multiple positions for the same transporter
        var positions = new List<TransporterPosition>
        {
            CreatePosition(transporterId, 10.0, 20.0, speed: 50.0),   // First update
            CreatePosition(transporterId, 30.0, 40.0, speed: 75.0),   // Second update
            CreatePosition(transporterId, 99.0, 99.0, speed: 999.0)   // Last - should be used for update
        };

        // Act
        await context.TransporterPositions.BulkAddOrUpdateAsync(
            positions,
            p => p.TransporterId,
            ["TransporterPositionId"],
            CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await context.TransporterPositions.ToListAsync();
        Assert.That(result, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].Latitude, Is.EqualTo(99.0));
            Assert.That(result[0].Speed, Is.EqualTo(999.0));
        }
    }

    [Test]
    public async Task BulkAddOrUpdateAsync_EmptyCollection_DoesNothing()
    {
        // Arrange
        var dbName = nameof(BulkAddOrUpdateAsync_EmptyCollection_DoesNothing);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using var context = new ApplicationDbContext(options);

        // Act
        await context.TransporterPositions.BulkAddOrUpdateAsync(
            [],
            p => p.TransporterId,
            ["TransporterPositionId"],
            CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await context.TransporterPositions.ToListAsync();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task BulkAddOrUpdateAsync_OnlyUpdatesChangedProperties()
    {
        // Arrange
        var dbName = nameof(BulkAddOrUpdateAsync_OnlyUpdatesChangedProperties);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var transporterId = Guid.NewGuid();

        await using var context = new ApplicationDbContext(options);

        // Add existing position
        var existingPosition = CreatePosition(transporterId, 10.0, 20.0, speed: 50.0, city: "OldCity");
        await context.TransporterPositions.AddAsync(existingPosition);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create position with same latitude but different city
        var updatedPosition = CreatePosition(transporterId, 10.0, 20.0, speed: 50.0, city: "NewCity");

        // Act
        await context.TransporterPositions.BulkAddOrUpdateAsync(
            [updatedPosition],
            p => p.TransporterId,
            ["TransporterPositionId"],
            CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await context.TransporterPositions.FirstAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.City, Is.EqualTo("NewCity"));
            Assert.That(result.Latitude, Is.EqualTo(10.0)); // Unchanged
        }
    }

    [Test]
    public async Task BulkAddOrUpdateAsync_HandlesAttributes_Correctly()
    {
        // Arrange
        var dbName = nameof(BulkAddOrUpdateAsync_HandlesAttributes_Correctly);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var transporterId = Guid.NewGuid();

        await using var context = new ApplicationDbContext(options);

        // Add existing position with attributes
        var existingPosition = CreatePosition(transporterId, 10.0, 20.0);
        existingPosition.Attributes = new AttributesVm(true, 10, 1000.0, 500.0, 25.0);
        await context.TransporterPositions.AddAsync(existingPosition);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create updated position with different attributes
        var updatedPosition = CreatePosition(transporterId, 10.0, 20.0);
        updatedPosition.Attributes = new AttributesVm(false, 15, 2000.0, 600.0, 30.0);

        // Act
        await context.TransporterPositions.BulkAddOrUpdateAsync(
            [updatedPosition],
            p => p.TransporterId,
            ["TransporterPositionId"],
            CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await context.TransporterPositions.FirstAsync();
        Assert.That(result.Attributes, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Attributes!.Value.Ignition, Is.False);
            Assert.That(result.Attributes!.Value.Satellites, Is.EqualTo(15));
            Assert.That(result.Attributes!.Value.Mileage, Is.EqualTo(2000.0));
        }
    }

    [Test]
    public async Task BulkAddOrUpdateAsync_MultipleDifferentTransporters_WithDuplicates_HandlesCorrectly()
    {
        // Arrange
        var dbName = nameof(BulkAddOrUpdateAsync_MultipleDifferentTransporters_WithDuplicates_HandlesCorrectly);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var transporterId1 = Guid.NewGuid();
        var transporterId2 = Guid.NewGuid();

        await using var context = new ApplicationDbContext(options);

        // Create positions with duplicates for each transporter
        var positions = new List<TransporterPosition>
        {
            CreatePosition(transporterId1, 10.0, 20.0),  // T1 first
            CreatePosition(transporterId2, 30.0, 40.0),  // T2 first
            CreatePosition(transporterId1, 50.0, 60.0),  // T1 last - should be kept
            CreatePosition(transporterId2, 70.0, 80.0)   // T2 last - should be kept
        };

        // Act
        await context.TransporterPositions.BulkAddOrUpdateAsync(
            positions,
            p => p.TransporterId,
            ["TransporterPositionId"],
            CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await context.TransporterPositions.ToListAsync();
        Assert.That(result, Has.Count.EqualTo(2));

        var pos1 = result.First(p => p.TransporterId == transporterId1);
        Assert.That(pos1.Latitude, Is.EqualTo(50.0));

        var pos2 = result.First(p => p.TransporterId == transporterId2);
        Assert.That(pos2.Latitude, Is.EqualTo(70.0));
    }

    private static TransporterPosition CreatePosition(
        Guid transporterId,
        double latitude,
        double longitude,
        double speed = 0.0,
        string? city = null)
    {
        return new TransporterPosition(
            transporterId,
            null,
            latitude,
            longitude,
            null,
            DateTimeOffset.UtcNow,
            speed,
            null,
            null,
            null,
            city,
            null,
            null,
            null);
    }
}
