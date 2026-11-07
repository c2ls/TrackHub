using Common.Application.Interfaces;
using Common.Infrastructure;
using Common.Infrastructure.Interceptors;
using Common.Mediator;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Events;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

/// <summary>
/// Spec 09 §10 commits to five workforce domain events. Nothing subscribes to them yet, so these
/// tests are the only thing standing between "declared in the spec" and "actually dispatched" —
/// without them a silently-dropped event would go unnoticed until the first consumer is written.
/// They run the REAL <see cref="DispatchDomainEventsInterceptor"/> against a capturing publisher.
/// </summary>
[TestFixture]
public class WorkforceDomainEventTests
{
    private List<BaseEvent> _published = null!;

    private ApplicationDbContext NewContext(string name)
    {
        _published = [];
        var publisher = new Mock<IPublisher>();
        publisher.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((n, _) => { if (n is BaseEvent e) { _published.Add(e); } })
            .Returns(Task.CompletedTask);

        return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .AddInterceptors(new DispatchDomainEventsInterceptor(publisher.Object))
            .Options);
    }

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(x => x.AccountId).Returns(accountId);
        principal.SetupGet(x => x.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        return principal.Object;
    }

    private static StaffMember SeedStaff(ApplicationDbContext context, Guid accountId)
    {
        var staff = new StaffMember(accountId, "Ana", null, null, null, true, null, null, null, null);
        context.StaffMembers.Add(staff);
        context.SaveChanges();
        return staff;
    }

    private static StaffQualificationDto Dto(Guid accountId, Guid staffId)
        => new(accountId, staffId, StaffQualificationTypes.License, "C2", "LIC-1", null, null, "RUNT",
            StaffQualificationStatuses.Valid, null, null);

    [Test]
    public async Task QualificationLifecycle_DispatchesCreatedUpdatedAndDeleted()
    {
        using var context = NewContext(nameof(QualificationLifecycle_DispatchesCreatedUpdatedAndDeleted));
        var accountId = Guid.NewGuid();
        var staff = SeedStaff(context, accountId);
        var writer = new StaffQualificationWriter(context, Principal(accountId), Mock.Of<IAlertRecorder>());

        var created = await writer.CreateStaffQualificationAsync(Dto(accountId, staff.StaffId), CancellationToken.None);
        Assert.That(_published.OfType<StaffQualificationCreatedEvent>().Single().StaffQualificationId,
            Is.EqualTo(created.StaffQualificationId));

        _published.Clear();
        await writer.UpdateStaffQualificationAsync(created.StaffQualificationId, Dto(accountId, staff.StaffId), CancellationToken.None);
        Assert.That(_published.OfType<StaffQualificationUpdatedEvent>().Count(), Is.EqualTo(1));

        // The delete case is the one at genuine risk: EF detaches removed entries during save, so an
        // event attached to a Removed entity could be dropped before the interceptor scans the tracker.
        _published.Clear();
        await writer.DeleteStaffQualificationAsync(created.StaffQualificationId, CancellationToken.None);
        var deleted = _published.OfType<StaffQualificationDeletedEvent>().SingleOrDefault();
        Assert.That(deleted, Is.Not.Null, "StaffQualificationDeleted was not dispatched — the removed entity was detached before the interceptor ran.");
        Assert.That(deleted!.StaffQualificationId, Is.EqualTo(created.StaffQualificationId));
    }

    [Test]
    public async Task AssignmentLifecycle_DispatchesAssignedAndEnded()
    {
        using var context = NewContext(nameof(AssignmentLifecycle_DispatchesAssignedAndEnded));
        var accountId = Guid.NewGuid();
        var staff = SeedStaff(context, accountId);
        var transporter = new Transporter("Truck 1", 1, accountId);
        context.Transporters.Add(transporter);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new StaffAssignmentWriter(context, Principal(accountId));

        var assignment = await writer.AssignStaffToTransporterAsync(staff.StaffId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddHours(-1), StaffAssignmentTypes.Regular, CancellationToken.None);
        var assigned = _published.OfType<StaffAssignedToTransporterEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(assigned.StaffTransporterAssignmentId, Is.EqualTo(assignment.StaffTransporterAssignmentId));
            Assert.That(assigned.TransporterId, Is.EqualTo(transporter.TransporterId));
        });

        _published.Clear();
        await writer.EndStaffAssignmentAsync(assignment.StaffTransporterAssignmentId, null, CancellationToken.None);
        Assert.That(_published.OfType<StaffAssignmentEndedEvent>().Single().StaffTransporterAssignmentId,
            Is.EqualTo(assignment.StaffTransporterAssignmentId));
    }
}
