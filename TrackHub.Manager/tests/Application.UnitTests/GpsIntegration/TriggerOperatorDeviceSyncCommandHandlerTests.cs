using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Common.Domain.Enums;
using TrackHub.Manager.Application.GpsIntegration.Commands;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.GpsIntegration;

[TestFixture]
public class TriggerOperatorDeviceSyncCommandHandlerTests
{
    private static bool IsGuid(string value) => Guid.TryParse(value, out var _);

    private static OperatorVm Operator(Guid operatorId, Guid accountId, DateTimeOffset? lastManualSyncAt = null)
        => new(
            operatorId,
            "Provider",
            null,
            null,
            null,
            null,
            null,
            ProtocolType.Traccar,
            2,
            accountId,
            DateTimeOffset.UtcNow,
            null,
            true,
            60,
            OperatorHealthStatus.Unknown,
            null,
            null,
            lastManualSyncAt,
            null,
            null,
            null,
            null,
            null);

    [Test]
    public async Task Handle_ReturnsDispatcherResultWithoutThrowingWhenRouterRejectsReset()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var operatorReader = new Mock<IOperatorReader>();
        var operatorWriter = new Mock<IOperatorWriter>();
        var dispatcher = new Mock<ISyncDispatcher>();
        var configuration = new Mock<IConfiguration>();
        operatorReader.Setup(x => x.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Operator(operatorId, accountId));
        dispatcher.Setup(x => x.DispatchManualSyncAsync(accountId, operatorId, It.IsAny<string>(), true, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        configuration.Setup(x => x["GpsIntegration:ManualSyncMinIntervalSeconds"])
            .Returns("0");
        var handler = new TriggerOperatorDeviceSyncCommandHandler(
            operatorReader.Object,
            operatorWriter.Object,
            dispatcher.Object,
            configuration.Object,
            Mock.Of<ILogger<TriggerOperatorDeviceSyncCommandHandler>>());

        var result = await handler.Handle(new TriggerOperatorDeviceSyncCommand(operatorId, ResetDeviceCatalog: true, AutoAssignNewDevices: true), CancellationToken.None);

        Assert.That(result, Is.False);
        dispatcher.Verify(x => x.DispatchManualSyncAsync(accountId, operatorId, It.Is<string>(c => IsGuid(c)), true, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_StampsManualSyncBeforeDispatch()
    {
        // Failed syncs must be throttled too: the stamp happens BEFORE dispatch, not only on
        // successful completion.
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var operatorReader = new Mock<IOperatorReader>();
        var operatorWriter = new Mock<IOperatorWriter>();
        var dispatcher = new Mock<ISyncDispatcher>();
        var configuration = new Mock<IConfiguration>();
        var sequence = new List<string>();
        operatorReader.Setup(x => x.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Operator(operatorId, accountId));
        operatorWriter.Setup(x => x.MarkManualSyncTriggeredAsync(operatorId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Callback(() => sequence.Add("stamp"))
            .Returns(Task.CompletedTask);
        dispatcher.Setup(x => x.DispatchManualSyncAsync(accountId, operatorId, It.IsAny<string>(), false, null, It.IsAny<CancellationToken>()))
            .Callback(() => sequence.Add("dispatch"))
            .ReturnsAsync(true);
        var handler = new TriggerOperatorDeviceSyncCommandHandler(
            operatorReader.Object,
            operatorWriter.Object,
            dispatcher.Object,
            configuration.Object,
            Mock.Of<ILogger<TriggerOperatorDeviceSyncCommandHandler>>());

        var result = await handler.Handle(new TriggerOperatorDeviceSyncCommand(operatorId), CancellationToken.None);

        Assert.That(result, Is.True);
        Assert.That(sequence, Is.EqualTo(new[] { "stamp", "dispatch" }));
    }

    [Test]
    public void Handle_WithinThrottleWindow_ThrowsTooManyRequests()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var operatorReader = new Mock<IOperatorReader>();
        var operatorWriter = new Mock<IOperatorWriter>();
        var dispatcher = new Mock<ISyncDispatcher>();
        var configuration = new Mock<IConfiguration>();
        operatorReader.Setup(x => x.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Operator(operatorId, accountId, lastManualSyncAt: DateTimeOffset.UtcNow.AddSeconds(-5)));
        configuration.Setup(x => x["GpsIntegration:ManualSyncMinIntervalSeconds"])
            .Returns("60");
        var handler = new TriggerOperatorDeviceSyncCommandHandler(
            operatorReader.Object,
            operatorWriter.Object,
            dispatcher.Object,
            configuration.Object,
            Mock.Of<ILogger<TriggerOperatorDeviceSyncCommandHandler>>());

        Assert.ThrowsAsync<Common.Application.Exceptions.TooManyRequestsException>(
            () => handler.Handle(new TriggerOperatorDeviceSyncCommand(operatorId), CancellationToken.None));
        operatorWriter.Verify(x => x.MarkManualSyncTriggeredAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        dispatcher.Verify(x => x.DispatchManualSyncAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
