using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Enums;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;

namespace Infrastructure.UnitTests;

// Coverage for the adoption lookup the device sync relies on: an account's same-name
// transporter with no ACTIVE device assignment is adoptable; anything actively assigned,
// differently named, or from another account is not.
[TestFixture]
public class TransporterReaderTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        return principal.Object;
    }

    private static TransporterDeviceAssignment AssignmentFor(Guid accountId, Guid transporterId, AssignmentStatus status)
        => new(accountId, transporterId, Guid.NewGuid(), DateTimeOffset.UtcNow, 0, true, (int)status, "test", "User");

    [Test]
    public async Task FindAdoptable_UnassignedSameName_ReturnsIt_CaseInsensitively()
    {
        await using var context = NewContext(nameof(FindAdoptable_UnassignedSameName_ReturnsIt_CaseInsensitively));
        var accountId = Guid.NewGuid();
        var transporter = new Transporter("ESR792", 1, accountId);
        await context.Transporters.AddAsync(transporter);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new TransporterReader(context as IApplicationDbContext, Principal(accountId));

        Assert.That(await reader.FindAdoptableTransporterAsync(accountId, "esr792", CancellationToken.None),
            Is.EqualTo(transporter.TransporterId));
    }

    [Test]
    public async Task FindAdoptable_ActivelyAssigned_ReturnsNull_ButEndedAssignmentDoesNotBlock()
    {
        await using var context = NewContext(nameof(FindAdoptable_ActivelyAssigned_ReturnsNull_ButEndedAssignmentDoesNotBlock));
        var accountId = Guid.NewGuid();
        var assigned = new Transporter("ESR792", 1, accountId);
        var ended = new Transporter("EXV184", 1, accountId);
        await context.Transporters.AddRangeAsync(assigned, ended);
        await context.TransporterDeviceAssignments.AddRangeAsync(
            AssignmentFor(accountId, assigned.TransporterId, AssignmentStatus.Active),
            AssignmentFor(accountId, ended.TransporterId, AssignmentStatus.Ended));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new TransporterReader(context as IApplicationDbContext, Principal(accountId));

        Assert.Multiple(async () =>
        {
            Assert.That(await reader.FindAdoptableTransporterAsync(accountId, "ESR792", CancellationToken.None), Is.Null);
            Assert.That(await reader.FindAdoptableTransporterAsync(accountId, "EXV184", CancellationToken.None),
                Is.EqualTo(ended.TransporterId));
        });
    }

    [Test]
    public async Task FindAdoptable_OtherAccountOrOtherName_ReturnsNull()
    {
        await using var context = NewContext(nameof(FindAdoptable_OtherAccountOrOtherName_ReturnsNull));
        var accountId = Guid.NewGuid();
        var otherAccountId = Guid.NewGuid();
        await context.Transporters.AddRangeAsync(
            new Transporter("ESR792", 1, otherAccountId),
            new Transporter("GTX122", 1, accountId));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new TransporterReader(context as IApplicationDbContext, Principal(accountId));

        Assert.That(await reader.FindAdoptableTransporterAsync(accountId, "ESR792", CancellationToken.None), Is.Null);
    }

    [Test]
    public async Task FindAdoptable_SeveralCandidates_PicksOldest()
    {
        await using var context = NewContext(nameof(FindAdoptable_SeveralCandidates_PicksOldest));
        var accountId = Guid.NewGuid();
        var older = new Transporter("ESR792", 1, accountId) { Created = DateTimeOffset.UtcNow.AddYears(-1) };
        var newer = new Transporter("ESR792", 1, accountId) { Created = DateTimeOffset.UtcNow };
        await context.Transporters.AddRangeAsync(newer, older);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new TransporterReader(context as IApplicationDbContext, Principal(accountId));

        Assert.That(await reader.FindAdoptableTransporterAsync(accountId, "ESR792", CancellationToken.None),
            Is.EqualTo(older.TransporterId));
    }
}
