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
using Microsoft.EntityFrameworkCore;
using TrackHub.Security.Domain.Records;
using TrackHub.Security.Infrastructure;
using TrackHub.Security.Infrastructure.Interfaces;
using TrackHub.Security.Infrastructure.Writers;

namespace Infrastructure.UnitTests;

// A duplicate service-client-permission grant (same client, account, resource,
// action, scope, audience) is rejected up-front with a 409 (ConflictException) — before it hits the
// unique index at SaveChanges.
[TestFixture]
public class ServiceClientPermissionWriterTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ServiceClientPermissionDto Dto(string action)
        => new("security_client", null, "Audit", action, "service_scope", "trackhub_api", true, null, null);

    [Test]
    public async Task Create_DuplicateFullKey_ThrowsConflict()
    {
        await using var context = NewContext(nameof(Create_DuplicateFullKey_ThrowsConflict));
        var writer = new ServiceClientPermissionWriter(context as IApplicationDbContext);
        await writer.CreateServiceClientPermissionAsync(Dto("Write"), CancellationToken.None);

        Assert.ThrowsAsync<ConflictException>(async () =>
            await writer.CreateServiceClientPermissionAsync(Dto("Write"), CancellationToken.None));
        Assert.That(context.ServiceClientPermissions.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Create_DistinctKey_Succeeds()
    {
        await using var context = NewContext(nameof(Create_DistinctKey_Succeeds));
        var writer = new ServiceClientPermissionWriter(context as IApplicationDbContext);

        await writer.CreateServiceClientPermissionAsync(Dto("Read"), CancellationToken.None);
        await writer.CreateServiceClientPermissionAsync(Dto("Write"), CancellationToken.None);

        Assert.That(context.ServiceClientPermissions.Count(), Is.EqualTo(2), "grants differing by action are not duplicates");
    }

    [Test]
    public async Task Update_ToCollidingKey_ThrowsConflict()
    {
        await using var context = NewContext(nameof(Update_ToCollidingKey_ThrowsConflict));
        var writer = new ServiceClientPermissionWriter(context as IApplicationDbContext);
        await writer.CreateServiceClientPermissionAsync(Dto("Read"), CancellationToken.None);
        var second = await writer.CreateServiceClientPermissionAsync(Dto("Write"), CancellationToken.None);

        // Re-point the second grant's action onto the first grant's key.
        Assert.ThrowsAsync<ConflictException>(async () =>
            await writer.UpdateServiceClientPermissionAsync(second.ServiceClientPermissionId, Dto("Read"), CancellationToken.None));
    }
}
