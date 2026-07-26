using Common.Application.Interfaces;
using Common.Infrastructure;
using Common.Infrastructure.Interceptors;
using Common.Mediator;
using Moq;
using TrackHub.Manager.Domain.Constants;
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
            .UseInMemoryDatabase(name)
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

    private static Driver SeedDriver(ApplicationDbContext context, Guid accountId)
    {
        var driver = new Driver(accountId, "Ana", null, null, null, true, null, null, null, null);
        context.Drivers.Add(driver);
        context.SaveChanges();
        return driver;
    }

    private static DriverQualificationDto Dto(Guid accountId, Guid driverId)
        => new(accountId, driverId, DriverQualificationTypes.License, "C2", "LIC-1", null, null, "RUNT",
            DriverQualificationStatuses.Valid, null, null);

    [Test]
    public async Task QualificationLifecycle_DispatchesCreatedUpdatedAndDeleted()
    {
        using var context = NewContext(nameof(QualificationLifecycle_DispatchesCreatedUpdatedAndDeleted));
        var accountId = Guid.NewGuid();
        var driver = SeedDriver(context, accountId);
        var writer = new DriverQualificationWriter(context, Principal(accountId));

        var created = await writer.CreateDriverQualificationAsync(Dto(accountId, driver.DriverId), CancellationToken.None);
        Assert.That(_published.OfType<DriverQualificationCreatedEvent>().Single().DriverQualificationId,
            Is.EqualTo(created.DriverQualificationId));

        _published.Clear();
        await writer.UpdateDriverQualificationAsync(created.DriverQualificationId, Dto(accountId, driver.DriverId), CancellationToken.None);
        Assert.That(_published.OfType<DriverQualificationUpdatedEvent>().Count(), Is.EqualTo(1));

        // The delete case is the one at genuine risk: EF detaches removed entries during save, so an
        // event attached to a Removed entity could be dropped before the interceptor scans the tracker.
        _published.Clear();
        await writer.DeleteDriverQualificationAsync(created.DriverQualificationId, CancellationToken.None);
        var deleted = _published.OfType<DriverQualificationDeletedEvent>().SingleOrDefault();
        Assert.That(deleted, Is.Not.Null, "DriverQualificationDeleted was not dispatched — the removed entity was detached before the interceptor ran.");
        Assert.That(deleted!.DriverQualificationId, Is.EqualTo(created.DriverQualificationId));
    }

    [Test]
    public async Task AssignmentLifecycle_DispatchesAssignedAndEnded()
    {
        using var context = NewContext(nameof(AssignmentLifecycle_DispatchesAssignedAndEnded));
        var accountId = Guid.NewGuid();
        var driver = SeedDriver(context, accountId);
        var transporter = new Transporter("Truck 1", 1, accountId);
        context.Transporters.Add(transporter);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new DriverAssignmentWriter(context, Principal(accountId));

        var assignment = await writer.AssignDriverToTransporterAsync(driver.DriverId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddHours(-1), DriverAssignmentTypes.Regular, CancellationToken.None);
        var assigned = _published.OfType<DriverAssignedToTransporterEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(assigned.DriverTransporterAssignmentId, Is.EqualTo(assignment.DriverTransporterAssignmentId));
            Assert.That(assigned.TransporterId, Is.EqualTo(transporter.TransporterId));
        });

        _published.Clear();
        await writer.EndDriverAssignmentAsync(assignment.DriverTransporterAssignmentId, null, CancellationToken.None);
        Assert.That(_published.OfType<DriverAssignmentEndedEvent>().Single().DriverTransporterAssignmentId,
            Is.EqualTo(assignment.DriverTransporterAssignmentId));
    }
}
