using System.Reflection;
using Common.Application.Attributes;
using Common.Application.Interfaces;
using TrackHub.Manager.Application.Drivers.Commands;
using TrackHub.Manager.Application.Drivers.Queries;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Drivers;

[TestFixture]
public class WorkforceHandlerTests
{
    private static Mock<ICurrentPrincipal> Principal(Guid? driverId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(x => x.DriverId).Returns(driverId);
        principal.SetupGet(x => x.PrincipalType).Returns(PrincipalType.Driver);
        return principal;
    }

    // AC2: the profile query must return the CALLER's data. The driver id comes from the principal and
    // is never accepted from the request, so a driver cannot ask for someone else's profile.
    [Test]
    public async Task GetMyDriverProfile_PinsDriverIdFromThePrincipal()
    {
        var callerDriverId = Guid.NewGuid();
        var reader = new Mock<IDriverReader>();
        reader.Setup(x => x.GetMyDriverProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyDriverProfileVm(default, [], []));

        var handler = new GetMyDriverProfileQueryHandler(reader.Object, Principal(callerDriverId).Object);
        await handler.Handle(new GetMyDriverProfileQuery(), CancellationToken.None);

        reader.Verify(x => x.GetMyDriverProfileAsync(callerDriverId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void GetMyDriverProfile_WithoutADriverIdentity_IsUnauthorized()
    {
        var handler = new GetMyDriverProfileQueryHandler(Mock.Of<IDriverReader>(), Principal(null).Object);

        Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(new GetMyDriverProfileQuery(), CancellationToken.None));
    }

    [Test]
    public async Task QualificationHandlers_DelegateToTheWriter()
    {
        var writer = new Mock<IDriverQualificationWriter>();
        var dto = new DriverQualificationDto(Guid.NewGuid(), Guid.NewGuid(), "License", null, null, null, null, null, "Valid", null, null);
        var id = Guid.NewGuid();

        await new CreateDriverQualificationCommandHandler(writer.Object).Handle(new CreateDriverQualificationCommand(dto), CancellationToken.None);
        await new UpdateDriverQualificationCommandHandler(writer.Object).Handle(new UpdateDriverQualificationCommand(id, dto), CancellationToken.None);
        await new DeleteDriverQualificationCommandHandler(writer.Object).Handle(new DeleteDriverQualificationCommand(id), CancellationToken.None);

        writer.Verify(x => x.CreateDriverQualificationAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
        writer.Verify(x => x.UpdateDriverQualificationAsync(id, dto, It.IsAny<CancellationToken>()), Times.Once);
        writer.Verify(x => x.DeleteDriverQualificationAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AssignmentHandlers_DelegateToTheWriter()
    {
        var writer = new Mock<IDriverAssignmentWriter>();
        var driverId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var startsAt = DateTimeOffset.UtcNow;
        var assignmentId = Guid.NewGuid();

        await new AssignDriverToTransporterCommandHandler(writer.Object)
            .Handle(new AssignDriverToTransporterCommand(driverId, transporterId, startsAt, "Regular"), CancellationToken.None);
        await new EndDriverAssignmentCommandHandler(writer.Object)
            .Handle(new EndDriverAssignmentCommand(assignmentId), CancellationToken.None);

        writer.Verify(x => x.AssignDriverToTransporterAsync(driverId, transporterId, startsAt, "Regular", It.IsAny<CancellationToken>()), Times.Once);
        writer.Verify(x => x.EndDriverAssignmentAsync(assignmentId, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// AC2: "every admin operation rejects driver principals". Authorization is enforced by
    /// <c>AuthorizationBehavior</c> reading <c>AuthorizeAttribute.PrincipalTypes</c>, whose DEFAULT is
    /// <c>"User,ServiceClient"</c> — a Driver principal is therefore rejected unless a surface opts in.
    /// This test pins that default across every workforce surface, so a future edit that adds
    /// <c>PrincipalTypes = "..."</c> including Driver, or a change to the attribute's default, fails here
    /// rather than silently exposing the driver registry to driver tokens.
    /// </summary>
    [Test]
    public void AdminSurfaces_DoNotAdmitDriverPrincipals()
    {
        Type[] adminSurfaces =
        [
            typeof(CreateDriverQualificationCommand), typeof(UpdateDriverQualificationCommand), typeof(DeleteDriverQualificationCommand),
            typeof(AssignDriverToTransporterCommand), typeof(EndDriverAssignmentCommand),
            typeof(GetDriverQualificationsQuery), typeof(GetDriverAssignmentHistoryQuery),
            typeof(CreateDriverCommand), typeof(UpdateDriverCommand), typeof(DeactivateDriverCommand),
            typeof(GetDriverQuery), typeof(GetDriversByAccountQuery), typeof(GetDriverAssignmentsQuery), typeof(ValidateDriverAssignmentQuery),
        ];

        Assert.Multiple(() =>
        {
            foreach (var surface in adminSurfaces)
            {
                var attribute = surface.GetCustomAttribute<AuthorizeAttribute>();
                Assert.That(attribute, Is.Not.Null, $"{surface.Name} carries no [Authorize].");
                Assert.That(attribute!.PrincipalTypes, Does.Not.Contain("Driver"), $"{surface.Name} admits Driver principals.");
                Assert.That(attribute.PrincipalTypes, Is.Not.Empty, $"{surface.Name} has an empty PrincipalTypes, which allows every principal type.");
            }
        });
    }

    [Test]
    public void MyDriverProfile_IsTheOnlyDriverFacingSurface()
    {
        var attribute = typeof(GetMyDriverProfileQuery).GetCustomAttribute<AuthorizeAttribute>();

        Assert.That(attribute!.PrincipalTypes, Is.EqualTo("Driver"));
    }
}
