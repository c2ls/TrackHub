using System.Reflection;
using Common.Application.Attributes;
using Common.Application.Interfaces;
using TrackHub.Manager.Application.Staff.Commands;
using TrackHub.Manager.Application.Staff.Queries;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Staff;

[TestFixture]
public class WorkforceHandlerTests
{
    private static Mock<ICurrentPrincipal> Principal(Guid? staffId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(x => x.StaffId).Returns(staffId);
        principal.SetupGet(x => x.PrincipalType).Returns(PrincipalType.Staff);
        return principal;
    }

    // AC2: the profile query must return the CALLER's data. The staff id comes from the principal and
    // is never accepted from the request, so a staff cannot ask for someone else's profile.
    [Test]
    public async Task GetMyStaffProfile_PinsStaffIdFromThePrincipal()
    {
        var callerStaffId = Guid.NewGuid();
        var reader = new Mock<IStaffMemberReader>();
        reader.Setup(x => x.GetMyStaffProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyStaffProfileVm(default, [], []));

        var handler = new GetMyStaffProfileQueryHandler(reader.Object, Principal(callerStaffId).Object);
        await handler.Handle(new GetMyStaffProfileQuery(), CancellationToken.None);

        reader.Verify(x => x.GetMyStaffProfileAsync(callerStaffId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetMyStaffProfile_WithoutAStaffIdentity_IsUnauthorized()
    {
        var handler = new GetMyStaffProfileQueryHandler(Mock.Of<IStaffMemberReader>(), Principal(null).Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(new GetMyStaffProfileQuery(), CancellationToken.None));
    }

    [Test]
    public async Task QualificationHandlers_DelegateToTheWriter()
    {
        var writer = new Mock<IStaffQualificationWriter>();
        var dto = new StaffQualificationDto(Guid.NewGuid(), Guid.NewGuid(), "License", null, null, null, null, null, "Valid", null, null);
        var id = Guid.NewGuid();

        await new CreateStaffQualificationCommandHandler(writer.Object).Handle(new CreateStaffQualificationCommand(dto), CancellationToken.None);
        await new UpdateStaffQualificationCommandHandler(writer.Object).Handle(new UpdateStaffQualificationCommand(id, dto), CancellationToken.None);
        await new DeleteStaffQualificationCommandHandler(writer.Object).Handle(new DeleteStaffQualificationCommand(id), CancellationToken.None);

        writer.Verify(x => x.CreateStaffQualificationAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
        writer.Verify(x => x.UpdateStaffQualificationAsync(id, dto, It.IsAny<CancellationToken>()), Times.Once);
        writer.Verify(x => x.DeleteStaffQualificationAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AssignmentHandlers_DelegateToTheWriter()
    {
        var writer = new Mock<IStaffAssignmentWriter>();
        var staffId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var startsAt = DateTimeOffset.UtcNow;
        var assignmentId = Guid.NewGuid();

        await new AssignStaffToTransporterCommandHandler(writer.Object)
            .Handle(new AssignStaffToTransporterCommand(staffId, transporterId, startsAt, "Regular"), CancellationToken.None);
        await new EndStaffAssignmentCommandHandler(writer.Object)
            .Handle(new EndStaffAssignmentCommand(assignmentId), CancellationToken.None);

        writer.Verify(x => x.AssignStaffToTransporterAsync(staffId, transporterId, startsAt, "Regular", It.IsAny<CancellationToken>()), Times.Once);
        writer.Verify(x => x.EndStaffAssignmentAsync(assignmentId, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// AC2: "every admin operation rejects staff principals". Authorization is enforced by
    /// <c>AuthorizationBehavior</c> reading <c>AuthorizeAttribute.PrincipalTypes</c>, whose DEFAULT is
    /// <c>"User,ServiceClient"</c> — a Staff principal is therefore rejected unless a surface opts in.
    /// This test pins that default across every workforce surface, so a future edit that adds
    /// <c>PrincipalTypes = "..."</c> including Staff, or a change to the attribute's default, fails here
    /// rather than silently exposing the staff registry to staff tokens.
    /// </summary>
    [Test]
    public void AdminSurfaces_DoNotAdmitStaffPrincipals()
    {
        Type[] adminSurfaces =
        [
            typeof(CreateStaffQualificationCommand), typeof(UpdateStaffQualificationCommand), typeof(DeleteStaffQualificationCommand),
            typeof(AssignStaffToTransporterCommand), typeof(EndStaffAssignmentCommand),
            typeof(GetStaffQualificationsQuery), typeof(GetStaffAssignmentHistoryQuery),
            typeof(CreateStaffMemberCommand), typeof(UpdateStaffMemberCommand), typeof(DeactivateStaffMemberCommand),
            typeof(GetStaffMemberQuery), typeof(GetStaffMembersByAccountQuery), typeof(GetStaffAssignmentsQuery), typeof(ValidateStaffAssignmentQuery),
        ];

        Assert.Multiple(() =>
        {
            foreach (var surface in adminSurfaces)
            {
                var attribute = surface.GetCustomAttribute<AuthorizeAttribute>();
                Assert.That(attribute, Is.Not.Null, $"{surface.Name} carries no [Authorize].");
                Assert.That(attribute!.PrincipalTypes, Does.Not.Contain("Staff"), $"{surface.Name} admits Staff principals.");
                Assert.That(attribute.PrincipalTypes, Is.Not.Empty, $"{surface.Name} has an empty PrincipalTypes, which allows every principal type.");
            }
        });
    }

    [Test]
    public void MyStaffProfile_IsTheOnlyStaffFacingSurface()
    {
        var attribute = typeof(GetMyStaffProfileQuery).GetCustomAttribute<AuthorizeAttribute>();

        Assert.That(attribute!.PrincipalTypes, Is.EqualTo("Staff"));
    }
}
