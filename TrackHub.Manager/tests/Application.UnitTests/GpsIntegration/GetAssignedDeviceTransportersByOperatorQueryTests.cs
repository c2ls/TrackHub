using System.Reflection;
using Common.Application.Attributes;
using Common.Domain.Constants;
using TrackHub.Manager.Application.GpsIntegration.Queries;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.GpsIntegration;

[TestFixture]
public class GetAssignedDeviceTransportersByOperatorQueryTests
{
    [Test]
    public async Task Handle_DelegatesToAccountScopedOperatorAssignmentReader()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var expected = new[]
        {
            new DeviceTransporterVm(Guid.NewGuid(), 123, "S-123", "Unit 123", Common.Domain.Enums.TransporterType.Car, 1)
        };
        var reader = new Mock<IDeviceTransporterReader>();
        reader.Setup(x => x.GetAssignedDeviceTransportersByOperatorAsync(accountId, operatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new GetAssignedDeviceTransportersByOperatorQueryHandler(reader.Object);

        var result = await handler.Handle(new GetAssignedDeviceTransportersByOperatorQuery(accountId, operatorId), CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));
        reader.Verify(x => x.GetAssignedDeviceTransportersByOperatorAsync(accountId, operatorId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Query_IsServiceClientOnlyAndNotFeatureGated()
    {
        var authorize = typeof(GetAssignedDeviceTransportersByOperatorQuery)
            .GetCustomAttribute<AuthorizeAttribute>();
        var featureAttributes = typeof(GetAssignedDeviceTransportersByOperatorQuery)
            .GetCustomAttributes()
            .Where(a => a.GetType().Name == "RequireFeatureAttribute");

        Assert.That(authorize, Is.Not.Null);
        Assert.That(authorize!.Resource, Is.EqualTo(Resources.SynchronizedDevices));
        Assert.That(authorize.Action, Is.EqualTo(Actions.Read));
        Assert.That(authorize.PrincipalTypes, Is.EqualTo("User,ServiceClient"));
        Assert.That(featureAttributes, Is.Empty);
    }
}
