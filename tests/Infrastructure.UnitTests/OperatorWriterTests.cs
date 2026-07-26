using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Enums;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class OperatorWriterTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    [Test]
    public async Task SetEnabledAsync_TogglesEnabledFlag()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(SetEnabledAsync_TogglesEnabledFlag));
        var op = new Operator("gps", null, null, null, null, null, 1, accountId) { Enabled = false };
        await context.Operators.AddAsync(op);
        await context.SaveChangesAsync();

        var writer = new OperatorWriter(context as IApplicationDbContext, ServicePrincipal());

        await writer.SetEnabledAsync(op.OperatorId, true, CancellationToken.None);
        var saved = await context.Operators.SingleAsync(x => x.OperatorId == op.OperatorId);
        Assert.That(saved.Enabled, Is.True);

        await writer.SetEnabledAsync(op.OperatorId, false, CancellationToken.None);
        saved = await context.Operators.SingleAsync(x => x.OperatorId == op.OperatorId);
        Assert.That(saved.Enabled, Is.False);
    }

    [Test]
    public async Task UpdateSyncSummaryAsync_ManualTrigger_StampsSuccessDeviceAndManualTimes()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(UpdateSyncSummaryAsync_ManualTrigger_StampsSuccessDeviceAndManualTimes));
        var op = new Operator("gps", null, null, null, null, null, 1, accountId);
        await context.Operators.AddAsync(op);
        await context.SaveChangesAsync();

        var writer = new OperatorWriter(context as IApplicationDbContext, ServicePrincipal());
        var finishedAt = DateTimeOffset.UtcNow;

        await writer.UpdateSyncSummaryAsync(op.OperatorId, finishedAt, SyncTriggerType.Manual, CancellationToken.None);

        var saved = await context.Operators.SingleAsync(x => x.OperatorId == op.OperatorId);
        Assert.Multiple(() =>
        {
            Assert.That(saved.LastSuccessfulSyncAt, Is.EqualTo(finishedAt));
            Assert.That(saved.LastDeviceSyncAt, Is.EqualTo(finishedAt));
            Assert.That(saved.LastManualSyncAt, Is.EqualTo(finishedAt));
        });
    }

    [Test]
    public async Task UpdateSyncSummaryAsync_AutomaticTrigger_DoesNotStampManualTime()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(UpdateSyncSummaryAsync_AutomaticTrigger_DoesNotStampManualTime));
        var op = new Operator("gps", null, null, null, null, null, 1, accountId);
        await context.Operators.AddAsync(op);
        await context.SaveChangesAsync();

        var writer = new OperatorWriter(context as IApplicationDbContext, ServicePrincipal());

        await writer.UpdateSyncSummaryAsync(op.OperatorId, DateTimeOffset.UtcNow, SyncTriggerType.Automatic, CancellationToken.None);

        var saved = await context.Operators.SingleAsync(x => x.OperatorId == op.OperatorId);
        Assert.Multiple(() =>
        {
            Assert.That(saved.LastSuccessfulSyncAt, Is.Not.Null);
            Assert.That(saved.LastManualSyncAt, Is.Null);
        });
    }

    [Test]
    public async Task MarkManualSyncTriggeredAsync_StampsManualTimeOnly()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(MarkManualSyncTriggeredAsync_StampsManualTimeOnly));
        var op = new Operator("gps", null, null, null, null, null, 1, accountId);
        await context.Operators.AddAsync(op);
        await context.SaveChangesAsync();

        var writer = new OperatorWriter(context as IApplicationDbContext, ServicePrincipal());
        var triggeredAt = DateTimeOffset.UtcNow;

        await writer.MarkManualSyncTriggeredAsync(op.OperatorId, triggeredAt, CancellationToken.None);

        var saved = await context.Operators.SingleAsync(x => x.OperatorId == op.OperatorId);
        Assert.Multiple(() =>
        {
            Assert.That(saved.LastManualSyncAt, Is.EqualTo(triggeredAt));
            Assert.That(saved.LastSuccessfulSyncAt, Is.Null);
            Assert.That(saved.LastDeviceSyncAt, Is.Null);
        });
    }
    // Account-transparent global service identity: these tests exercise sync stamping, while the
    // by-id account guard is pinned by AccountScopeGuardTests.
    private static ICurrentPrincipal ServicePrincipal()
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.ServiceClient);
        principal.SetupGet(p => p.AccountId).Returns((Guid?)null);
        return principal.Object;
    }
}
