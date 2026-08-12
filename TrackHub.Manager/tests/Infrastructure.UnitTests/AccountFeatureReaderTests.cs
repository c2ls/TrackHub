using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class AccountFeatureReaderTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid? accountId, PrincipalType principalType = PrincipalType.User, Guid? userId = null)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.PrincipalType).Returns(principalType);
        principal.SetupGet(p => p.UserId).Returns(userId);
        return principal.Object;
    }

    [Test]
    public async Task GetAccountFeaturesAsync_UserResolvedByAccountMembership_ReturnsFeatures()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetAccountFeaturesAsync_UserResolvedByAccountMembership_ReturnsFeatures));
        await context.Users.AddAsync(new User(userId, "alice", true, accountId));
        await context.AccountFeatures.AddAsync(new AccountFeature(accountId, "gps.integration", true, "standard", "manual", null, null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new AccountFeatureReader(context as IApplicationDbContext, Principal(null, userId: userId));
        var result = await reader.GetAccountFeaturesAsync(accountId, CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.Single().FeatureKey, Is.EqualTo("gps.integration"));
    }

    [Test]
    public async Task GetAccountFeaturesAsync_CrossAccountUser_ThrowsDirectForbidden()
    {
        var requestedAccountId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetAccountFeaturesAsync_CrossAccountUser_ThrowsDirectForbidden));
        await context.Users.AddAsync(new User(Guid.NewGuid(), "alice", true, userAccountId));
        await context.AccountFeatures.AddAsync(new AccountFeature(requestedAccountId, "gps.integration", true, "standard", "manual", null, null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new AccountFeatureReader(context as IApplicationDbContext, Principal(userAccountId));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await reader.GetAccountFeaturesAsync(requestedAccountId, CancellationToken.None));
    }

    [Test]
    public async Task GetAccountFeaturesAsync_GlobalServiceClient_ReturnsFeaturesForRequestedAccount()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetAccountFeaturesAsync_GlobalServiceClient_ReturnsFeaturesForRequestedAccount));
        await context.AccountFeatures.AddAsync(new AccountFeature(accountId, "gps.integration", true, "standard", "manual", null, null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new AccountFeatureReader(context as IApplicationDbContext, Principal(null, PrincipalType.ServiceClient));
        var result = await reader.GetAccountFeaturesAsync(accountId, CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.Single().AccountId, Is.EqualTo(accountId));
    }
}
