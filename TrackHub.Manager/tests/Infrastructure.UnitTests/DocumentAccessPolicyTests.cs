using Common.Application.Interfaces;
using Common.Domain.Constants;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class DocumentAccessPolicyTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static Mock<ICurrentPrincipal> PrincipalMock(Guid accountId, PrincipalType type = PrincipalType.User, string? role = null, Guid? userId = null, Guid? driverId = null)
    {
        var p = new Mock<ICurrentPrincipal>();
        p.SetupGet(x => x.AccountId).Returns(accountId);
        p.SetupGet(x => x.PrincipalType).Returns(type);
        p.SetupGet(x => x.Role).Returns(role);
        p.SetupGet(x => x.UserId).Returns(userId);
        p.SetupGet(x => x.DriverId).Returns(driverId);
        return p;
    }

    private static DocumentAccessPolicy NewPolicy(ApplicationDbContext context, ICurrentPrincipal principal,
        IVisibleTransporterReader? visible = null, IDriverReader? driver = null)
        => new(context, principal, visible ?? Mock.Of<IVisibleTransporterReader>(), driver ?? Mock.Of<IDriverReader>());

    [Test]
    public void IsClearedForClassification_PublicAlways_SensitiveNeedsPrivilege()
    {
        using var context = NewContext(nameof(IsClearedForClassification_PublicAlways_SensitiveNeedsPrivilege));
        var accountId = Guid.NewGuid();

        var plain = NewPolicy(context, PrincipalMock(accountId, role: Roles.User).Object);
        Assert.That(plain.IsClearedForClassification(DocumentClassifications.Public), Is.True);
        Assert.That(plain.IsClearedForClassification(DocumentClassifications.Confidential), Is.False);
        Assert.That(plain.IsClearedForClassification(DocumentClassifications.Legal), Is.False);

        var admin = NewPolicy(context, PrincipalMock(accountId, role: Roles.Administrator).Object);
        Assert.That(admin.IsClearedForClassification(DocumentClassifications.Confidential), Is.True);
    }

    [Test]
    public async Task CanAccessOwner_UnknownOwnerType_DenyByDefault()
    {
        using var context = NewContext(nameof(CanAccessOwner_UnknownOwnerType_DenyByDefault));
        var accountId = Guid.NewGuid();
        var policy = NewPolicy(context, PrincipalMock(accountId, role: Roles.Administrator).Object);

        var ok = await policy.CanAccessOwnerAsync(accountId, "Trip", Guid.NewGuid().ToString(), forWrite: false, CancellationToken.None);
        Assert.That(ok, Is.False);
    }

    [Test]
    public async Task CanAccessOwner_DriverPrincipal_Transporter_UsesAssignment()
    {
        using var context = NewContext(nameof(CanAccessOwner_DriverPrincipal_Transporter_UsesAssignment));
        var accountId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();

        var driverReader = new Mock<IDriverReader>();
        driverReader.Setup(r => r.ValidateDriverAssignmentAsync(driverId, DocumentOwnerTypes.Transporter, transporterId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var policy = NewPolicy(context, PrincipalMock(accountId, PrincipalType.Driver, driverId: driverId).Object, driver: driverReader.Object);

        Assert.That(await policy.CanAccessOwnerAsync(accountId, DocumentOwnerTypes.Transporter, transporterId.ToString(), false, CancellationToken.None), Is.True);
        Assert.That(await policy.CanAccessOwnerAsync(accountId, DocumentOwnerTypes.Transporter, Guid.NewGuid().ToString(), false, CancellationToken.None), Is.False);
    }

    [Test]
    public async Task CanAccessOwner_DriverOwner_OnlySelf()
    {
        using var context = NewContext(nameof(CanAccessOwner_DriverOwner_OnlySelf));
        var accountId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var policy = NewPolicy(context, PrincipalMock(accountId, PrincipalType.Driver, driverId: driverId).Object);

        Assert.That(await policy.CanAccessOwnerAsync(accountId, DocumentOwnerTypes.Driver, driverId.ToString(), false, CancellationToken.None), Is.True);
        Assert.That(await policy.CanAccessOwnerAsync(accountId, DocumentOwnerTypes.Driver, Guid.NewGuid().ToString(), false, CancellationToken.None), Is.False);
    }

    [Test]
    public async Task CanAccessOwner_User_Transporter_UsesVisibleSet()
    {
        using var context = NewContext(nameof(CanAccessOwner_User_Transporter_UsesVisibleSet));
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var visibleId = Guid.NewGuid();

        var visible = new Mock<IVisibleTransporterReader>();
        visible.Setup(r => r.GetVisibleTransporterIdsAsync(userId, accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<Guid> { visibleId });

        var policy = NewPolicy(context, PrincipalMock(accountId, PrincipalType.User, role: Roles.User, userId: userId).Object, visible: visible.Object);

        Assert.That(await policy.CanAccessOwnerAsync(accountId, DocumentOwnerTypes.Transporter, visibleId.ToString(), false, CancellationToken.None), Is.True);
        Assert.That(await policy.CanAccessOwnerAsync(accountId, DocumentOwnerTypes.Transporter, Guid.NewGuid().ToString(), false, CancellationToken.None), Is.False);
    }
}
