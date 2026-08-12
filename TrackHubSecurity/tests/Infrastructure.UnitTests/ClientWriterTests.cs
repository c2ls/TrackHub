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
using TrackHub.Security.Infrastructure.SecurityDB;
using TrackHub.Security.Infrastructure.SecurityDB.Entities;
using TrackHub.Security.Infrastructure.SecurityDB.Interfaces;
using TrackHub.Security.Infrastructure.SecurityDB.Writers;

namespace Infrastructure.UnitTests;

// Client name is unique (409 on duplicate) and the create response echoes the
// supplied secret once while the stored column holds ciphertext, never plaintext.
[TestFixture]
public class ClientWriterTests
{
    private const string Key = "test-key";
    private static readonly byte[] Salt = [1, 2, 3, 4, 5, 6, 7, 8];

    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    [Test]
    public async Task CreateClientAsync_DistinctName_EchoesSecretOnce_ButStoresCiphertext()
    {
        await using var context = NewContext(nameof(CreateClientAsync_DistinctName_EchoesSecretOnce_ButStoresCiphertext));
        var writer = new ClientWriter(context as IApplicationDbContext);
        var dto = new ClientDto(null, "integration_client", "desc", "s3cr3t");

        var vm = await writer.CreateClientAsync(dto, Salt, Key, CancellationToken.None);

        Assert.That(vm.Secret, Is.EqualTo("s3cr3t"), "the create response echoes the plaintext secret exactly once");
        var stored = context.Clients.Single();
        Assert.That(stored.Secret, Is.Not.EqualTo("s3cr3t"), "the persisted secret is encrypted, never plaintext");
    }

    [Test]
    public async Task CreateClientAsync_DuplicateName_ThrowsConflict()
    {
        await using var context = NewContext(nameof(CreateClientAsync_DuplicateName_ThrowsConflict));
        await context.Clients.AddAsync(new Client("integration_client", null, "desc", "cipher", Convert.ToBase64String(Salt), false));
        await context.SaveChangesAsync(CancellationToken.None);
        var writer = new ClientWriter(context as IApplicationDbContext);
        var dto = new ClientDto(null, "integration_client", "desc", "s3cr3t");

        Assert.ThrowsAsync<ConflictException>(async () =>
            await writer.CreateClientAsync(dto, Salt, Key, CancellationToken.None));
        Assert.That(context.Clients.Count(), Is.EqualTo(1), "the duplicate name must not create a second client");
    }
}
