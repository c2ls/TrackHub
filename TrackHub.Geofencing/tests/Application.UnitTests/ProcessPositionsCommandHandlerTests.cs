using TrackHub.Geofencing.Application.GeofenceEvents.Commands.ProcessPositions;
using TrackHub.Geofencing.Application.GeofenceEvents.Services.Interfaces;
using TrackHub.Geofencing.Domain.Interfaces;
using TrackHub.Geofencing.Domain.Records;

namespace TrackHub.Geofencing.Application.UnitTests.GeofenceEvents.Commands.ProcessPositions;

[TestFixture]
public class ProcessPositionsCommandHandlerTests
{
    private Mock<IGeofenceDetectionService> _detectionServiceMock = null!;
    private Mock<IAccountFeatureReader> _featureReaderMock = null!;

    [SetUp]
    public void SetUp()
    {
        _detectionServiceMock = new Mock<IGeofenceDetectionService>();
        _featureReaderMock = new Mock<IAccountFeatureReader>();
    }

    [Test]
    public async Task Handle_ShouldCallDetectionServiceAndReturnResult()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var positions = new[] { new TransporterPositionDto(transporterId, 1.0, 2.0, DateTimeOffset.UtcNow) };
        var expected = new GeofenceProcessingResultVm(positions.Length, 1, 0);

        _detectionServiceMock
            .Setup(s => s.ProcessPositionsAsync(It.IsAny<IEnumerable<TransporterPositionDto>>(), accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        _featureReaderMock.Setup(r => r.EnsureFeatureEnabledAsync(accountId, FeatureKeys.Geofencing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new ProcessPositionsCommandHandler(_detectionServiceMock.Object, _featureReaderMock.Object);
        var command = new ProcessPositionsCommand(accountId, positions);
        var cts = new CancellationTokenSource();

        // Act
        var result = await handler.Handle(command, cts.Token);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
        _featureReaderMock.Verify(r => r.EnsureFeatureEnabledAsync(accountId, FeatureKeys.Geofencing, It.IsAny<CancellationToken>()), Times.Once);
        _detectionServiceMock.Verify(s => s.ProcessPositionsAsync(
            It.Is<IEnumerable<TransporterPositionDto>>(p => p != null && p.SequenceEqual(positions)),
            accountId,
            It.Is<CancellationToken>(ct => ct == cts.Token)), Times.Once);
    }
}

