using Common.Application.Interfaces;
using Common.Domain.Extensions;
using Moq;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class CredentialWriterTests
{
    private const string EncryptionKey = "test-encryption-key";

    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        return principal.Object;
    }

    private static Credential CredentialFor(Operator @operator)
    {
        var salt = new byte[] { 9, 18, 27, 36, 45, 54, 63, 72 };
        return new Credential(
            "https://provider.example",
            "operator-user".EncryptData(EncryptionKey, salt),
            "operator-pass".EncryptData(EncryptionKey, salt),
            null,
            null,
            Convert.ToBase64String(salt),
            @operator.OperatorId)
        {
            Operator = @operator
        };
    }

    [Test]
    public async Task CreateCredentialAsync_PrincipalFromDifferentAccount_ThrowsForbidden()
    {
        await using var context = NewContext(nameof(CreateCredentialAsync_PrincipalFromDifferentAccount_ThrowsForbidden));
        var @operator = new Operator("Provider", null, null, null, null, null, 1, Guid.NewGuid());
        await context.Operators.AddAsync(@operator);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new CredentialWriter(context as IApplicationDbContext, Principal(Guid.NewGuid()));
        var dto = new CredentialDto("https://provider.example", "user", "pass", null, null, @operator.OperatorId);

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await writer.CreateCredentialAsync(dto, [1, 2, 3, 4, 5, 6, 7, 8], EncryptionKey, CancellationToken.None));
    }

    [Test]
    public async Task UpdateCredentialAsync_PrincipalFromDifferentAccount_ThrowsForbidden()
    {
        await using var context = NewContext(nameof(UpdateCredentialAsync_PrincipalFromDifferentAccount_ThrowsForbidden));
        var @operator = new Operator("Provider", null, null, null, null, null, 1, Guid.NewGuid());
        var credential = CredentialFor(@operator);
        await context.Operators.AddAsync(@operator);
        await context.Credentials.AddAsync(credential);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new CredentialWriter(context as IApplicationDbContext, Principal(Guid.NewGuid()));
        var dto = new UpdateCredentialDto(credential.CredentialId, "https://changed.example", "new-user", "new-pass", "new-key", "new-key-2");

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await writer.UpdateCredentialAsync(dto, [1, 2, 3, 4, 5, 6, 7, 8], EncryptionKey, CancellationToken.None));
    }

    [Test]
    public async Task UpdateTokenAsync_PrincipalFromOperatorAccount_UpdatesToken()
    {
        await using var context = NewContext(nameof(UpdateTokenAsync_PrincipalFromOperatorAccount_UpdatesToken));
        var accountId = Guid.NewGuid();
        var @operator = new Operator("Provider", null, null, null, null, null, 1, accountId);
        var credential = CredentialFor(@operator);
        await context.Operators.AddAsync(@operator);
        await context.Credentials.AddAsync(credential);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new CredentialWriter(context as IApplicationDbContext, Principal(accountId));
        var dto = new UpdateTokenDto(credential.CredentialId, "token", DateTimeOffset.UtcNow.AddHours(1), "refresh", DateTimeOffset.UtcNow.AddDays(1));

        await writer.UpdateTokenAsync(dto, EncryptionKey, CancellationToken.None);

        var updated = await context.Credentials.SingleAsync(c => c.CredentialId == credential.CredentialId);
        var salt = Convert.FromBase64String(updated.Salt);
        Assert.That(updated.Token?.DecryptData(EncryptionKey, salt), Is.EqualTo("token"));
        Assert.That(updated.RefreshToken?.DecryptData(EncryptionKey, salt), Is.EqualTo("refresh"));
    }

    [Test]
    public async Task UpdateTokenAsync_PrincipalFromDifferentAccount_ThrowsForbidden()
    {
        await using var context = NewContext(nameof(UpdateTokenAsync_PrincipalFromDifferentAccount_ThrowsForbidden));
        var @operator = new Operator("Provider", null, null, null, null, null, 1, Guid.NewGuid());
        var credential = CredentialFor(@operator);
        await context.Operators.AddAsync(@operator);
        await context.Credentials.AddAsync(credential);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new CredentialWriter(context as IApplicationDbContext, Principal(Guid.NewGuid()));
        var dto = new UpdateTokenDto(credential.CredentialId, "token", DateTimeOffset.UtcNow.AddHours(1), "refresh", DateTimeOffset.UtcNow.AddDays(1));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await writer.UpdateTokenAsync(dto, EncryptionKey, CancellationToken.None));
    }

    [Test]
    public async Task UpdateTokenAsync_GlobalServiceClientWithoutAccount_DoesNotRequireSpecificAccount()
    {
        await using var context = NewContext(nameof(UpdateTokenAsync_GlobalServiceClientWithoutAccount_DoesNotRequireSpecificAccount));
        var @operator = new Operator("Provider", null, null, null, null, null, 1, Guid.NewGuid());
        var credential = CredentialFor(@operator);
        await context.Operators.AddAsync(@operator);
        await context.Credentials.AddAsync(credential);
        await context.SaveChangesAsync(CancellationToken.None);
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.ServiceClient);

        var writer = new CredentialWriter(context as IApplicationDbContext, principal.Object);
        var dto = new UpdateTokenDto(credential.CredentialId, "token", DateTimeOffset.UtcNow.AddHours(1), "refresh", DateTimeOffset.UtcNow.AddDays(1));

        await writer.UpdateTokenAsync(dto, EncryptionKey, CancellationToken.None);

        var updated = await context.Credentials.SingleAsync(c => c.CredentialId == credential.CredentialId);
        Assert.That(updated.Token, Is.Not.Null);
        Assert.That(updated.RefreshToken, Is.Not.Null);
    }
}
