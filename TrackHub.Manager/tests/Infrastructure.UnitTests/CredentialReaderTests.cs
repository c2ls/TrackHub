using Common.Application.Interfaces;
using Common.Domain.Extensions;
using Moq;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class CredentialReaderTests
{
    private const string EncryptionKey = "test-encryption-key";

    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid? accountId, PrincipalType principalType = PrincipalType.User)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.PrincipalType).Returns(principalType);
        return principal.Object;
    }

    private static Credential CredentialFor(Operator @operator)
    {
        var salt = new byte[] { 11, 22, 33, 44, 55, 66, 77, 88 };
        return new Credential(
            "https://provider.example",
            "operator-user".EncryptData(EncryptionKey, salt),
            "operator-pass".EncryptData(EncryptionKey, salt),
            "api-key".EncryptData(EncryptionKey, salt),
            null,
            Convert.ToBase64String(salt),
            @operator.OperatorId)
        {
            Operator = @operator
        };
    }

    [Test]
    public async Task GetCredentialAsync_PrincipalFromCredentialAccount_ReturnsDecryptedCredential()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetCredentialAsync_PrincipalFromCredentialAccount_ReturnsDecryptedCredential));
        var @operator = new Operator("Provider", null, null, null, null, null, 1, accountId);
        var credential = CredentialFor(@operator);
        await context.Operators.AddAsync(@operator);
        await context.Credentials.AddAsync(credential);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new CredentialReader(context as IApplicationDbContext, Principal(accountId));
        var result = await reader.GetCredentialAsync(credential.CredentialId, EncryptionKey, CancellationToken.None);

        Assert.That(result.CredentialId, Is.EqualTo(credential.CredentialId));
        Assert.That(result.Username, Is.EqualTo("operator-user"));
        Assert.That(result.Password, Is.EqualTo("operator-pass"));
        Assert.That(result.Key, Is.EqualTo("api-key"));
    }

    [Test]
    public async Task GetCredentialAsync_PrincipalFromDifferentAccount_ThrowsForbidden()
    {
        await using var context = NewContext(nameof(GetCredentialAsync_PrincipalFromDifferentAccount_ThrowsForbidden));
        var @operator = new Operator("Provider", null, null, null, null, null, 1, Guid.NewGuid());
        var credential = CredentialFor(@operator);
        await context.Operators.AddAsync(@operator);
        await context.Credentials.AddAsync(credential);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new CredentialReader(context as IApplicationDbContext, Principal(Guid.NewGuid()));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await reader.GetCredentialAsync(credential.CredentialId, EncryptionKey, CancellationToken.None));
    }

    [Test]
    public async Task GetMetadataByOperatorAsync_PrincipalFromDifferentAccount_ThrowsForbidden()
    {
        await using var context = NewContext(nameof(GetMetadataByOperatorAsync_PrincipalFromDifferentAccount_ThrowsForbidden));
        var @operator = new Operator("Provider", null, null, null, null, null, 1, Guid.NewGuid());
        var credential = CredentialFor(@operator);
        await context.Operators.AddAsync(@operator);
        await context.Credentials.AddAsync(credential);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new CredentialReader(context as IApplicationDbContext, Principal(Guid.NewGuid()));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await reader.GetMetadataByOperatorAsync(@operator.OperatorId, CancellationToken.None));
    }

    [Test]
    public async Task GetTokenAsync_GlobalServiceClient_ReturnsDecryptedToken()
    {
        var accountId = Guid.NewGuid();
        var salt = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };
        await using var context = NewContext(nameof(GetTokenAsync_GlobalServiceClient_ReturnsDecryptedToken));
        var @operator = new Operator("Provider", null, null, null, null, null, 1, accountId);
        var credential = new Credential(
            "https://provider.example",
            "operator-user".EncryptData(EncryptionKey, salt),
            "operator-pass".EncryptData(EncryptionKey, salt),
            null,
            null,
            Convert.ToBase64String(salt),
            @operator.OperatorId)
        {
            Operator = @operator,
            Token = "access-token".EncryptData(EncryptionKey, salt),
            RefreshToken = "refresh-token".EncryptData(EncryptionKey, salt),
            TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(30),
            RefreshTokenExpiration = DateTimeOffset.UtcNow.AddDays(1)
        };
        await context.Operators.AddAsync(@operator);
        await context.Credentials.AddAsync(credential);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new CredentialReader(context as IApplicationDbContext, Principal(null, PrincipalType.ServiceClient));
        var result = await reader.GetTokenAsync(credential.CredentialId, EncryptionKey, CancellationToken.None);

        Assert.That(result.Token, Is.EqualTo("access-token"));
        Assert.That(result.RefreshToken, Is.EqualTo("refresh-token"));
    }
}
