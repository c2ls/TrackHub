using Microsoft.Extensions.Logging;
using TrackHub.Geofencing.Application.GeofenceEvents.Services;
using TrackHub.Geofencing.Domain.Interfaces;
using TrackHub.Geofencing.Domain.Records;

namespace TrackHub.Geofencing.Application.UnitTests.GeofenceEvents.Services;

[TestFixture]
public class GeofenceDetectionServiceTests
{
    private Mock<IGeofenceReader> _geofenceReaderMock = null!;
    private Mock<IGeofenceEventReader> _geofenceEventReaderMock = null!;
    private Mock<IGeofenceEventWriter> _geofenceEventWriterMock = null!;
    private Mock<IAlertEmitter> _alertEmitterMock = null!;

    [SetUp]
    public void SetUp()
    {
        // Create fresh mocks per test to ensure isolation
        _geofenceReaderMock = new Mock<IGeofenceReader>();
        _geofenceEventReaderMock = new Mock<IGeofenceEventReader>();
        _geofenceEventWriterMock = new Mock<IGeofenceEventWriter>();
        _alertEmitterMock = new Mock<IAlertEmitter>();

        // Default: no alert opt-in metadata, so detection creates events but emits nothing.
        _geofenceReaderMock.Setup(r => r.GetActiveGeofenceAlertInfoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, GeofenceAlertInfoVm>());
    }

    private GeofenceDetectionService CreateService() => new(
        _geofenceReaderMock.Object,
        _geofenceEventReaderMock.Object,
        _geofenceEventWriterMock.Object,
        _alertEmitterMock.Object,
        Mock.Of<ILogger<GeofenceDetectionService>>());

    [Test]
    public async Task ProcessPositionsAsync_NoPositions_ReturnsZeros()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.ProcessPositionsAsync([], Guid.NewGuid(), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.ProcessedCount, Is.Zero);
            Assert.That(result.EventsCreated, Is.Zero);
            Assert.That(result.EventsUpdated, Is.Zero);
        }
    }

    [Test]
    public async Task ProcessPositionsAsync_SinglePosition_EntersGeofence_CreatesEvent()
    {
        // Arrange
        var transporterId = Guid.NewGuid();
        var geofenceId = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow;

        var position = new TransporterPositionDto(transporterId, 10.0, 20.0, timestamp);

        _geofenceEventReaderMock.Setup(r => r.GetOpenEventsForTransporterAsync(transporterId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _geofenceReaderMock.Setup(r => r.GetGeofenceIdsContainingPointAsync(It.IsAny<Guid>(), position.Latitude, position.Longitude, It.IsAny<CancellationToken>()))
            .ReturnsAsync([geofenceId]);

        var createdEvent = new GeofenceEventVm(Guid.NewGuid(), transporterId, geofenceId, timestamp, null, position.Latitude, position.Longitude);
        _geofenceEventWriterMock.Setup(w => w.CreateEntryEventAsync(It.IsAny<GeofenceEventDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEvent);

        var service = CreateService();

        // Act
        var result = await service.ProcessPositionsAsync([position], Guid.NewGuid(), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.ProcessedCount, Is.EqualTo(1));
            Assert.That(result.EventsCreated, Is.EqualTo(1));
            Assert.That(result.EventsUpdated, Is.Zero);
        }
        _geofenceEventWriterMock.Verify(w => w.CreateEntryEventAsync(It.IsAny<GeofenceEventDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ProcessPositionsAsync_EntryThenExit_UpdatesEvent()
    {
        // Arrange
        var transporterId = Guid.NewGuid();
        var geofenceId = Guid.NewGuid();
        var t0 = DateTimeOffset.UtcNow;
        var t1 = t0.AddSeconds(60);

        var posEntry = new TransporterPositionDto(transporterId, 10.0, 20.0, t0);
        var posExit = new TransporterPositionDto(transporterId, 11.0, 21.0, t1);

        // No open events initially
        _geofenceEventReaderMock.Setup(r => r.GetOpenEventsForTransporterAsync(transporterId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // First call returns the geofence id, second call returns empty
        _geofenceReaderMock.SetupSequence(r => r.GetGeofenceIdsContainingPointAsync(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([geofenceId])
            .ReturnsAsync([]);

        var createdEventId = Guid.NewGuid();
        var createdEvent = new GeofenceEventVm(createdEventId, transporterId, geofenceId, t0, null, posEntry.Latitude, posEntry.Longitude);
        _geofenceEventWriterMock.Setup(w => w.CreateEntryEventAsync(It.IsAny<GeofenceEventDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEvent);

        var closedEvent = new GeofenceEventVm(createdEventId, transporterId, geofenceId, t0, t1, posEntry.Latitude, posEntry.Longitude);
        _geofenceEventWriterMock.Setup(w => w.UpdateExitEventAsync(createdEventId, t1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(closedEvent);

        var service = CreateService();

        // Act
        var result = await service.ProcessPositionsAsync([posEntry, posExit], Guid.NewGuid(), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.ProcessedCount, Is.EqualTo(2));
            Assert.That(result.EventsCreated, Is.EqualTo(1));
            Assert.That(result.EventsUpdated, Is.EqualTo(1));
        }
        _geofenceEventWriterMock.Verify(w => w.CreateEntryEventAsync(It.IsAny<GeofenceEventDto>(), It.IsAny<CancellationToken>()), Times.Once);
        _geofenceEventWriterMock.Verify(w => w.UpdateExitEventAsync(createdEventId, t1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ProcessPositionsAsync_ExitDebounce_DoesNotUpdate_WhenWithinInterval()
    {
        // Arrange
        var transporterId = Guid.NewGuid();
        var geofenceId = Guid.NewGuid();
        var t0 = DateTimeOffset.UtcNow;
        var t1 = t0.AddSeconds(10); // less than debounce interval (30s)

        var posEntry = new TransporterPositionDto(transporterId, 10.0, 20.0, t0);
        var posExit = new TransporterPositionDto(transporterId, 11.0, 21.0, t1);

        _geofenceEventReaderMock.Setup(r => r.GetOpenEventsForTransporterAsync(transporterId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _geofenceReaderMock.SetupSequence(r => r.GetGeofenceIdsContainingPointAsync(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([geofenceId])
            .ReturnsAsync([]);

        var createdEventId = Guid.NewGuid();
        var createdEvent = new GeofenceEventVm(createdEventId, transporterId, geofenceId, t0, null, posEntry.Latitude, posEntry.Longitude);
        _geofenceEventWriterMock.Setup(w => w.CreateEntryEventAsync(It.IsAny<GeofenceEventDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEvent);

        var service = CreateService();

        // Act
        var result = await service.ProcessPositionsAsync([posEntry, posExit], Guid.NewGuid(), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.ProcessedCount, Is.EqualTo(2));
            Assert.That(result.EventsCreated, Is.EqualTo(1));
            Assert.That(result.EventsUpdated, Is.Zero);
        }
        _geofenceEventWriterMock.Verify(w => w.UpdateExitEventAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ProcessPositionsAsync_MultipleGeofences_EntryCreatesMultipleEvents()
    {
        // Arrange
        var transporterId = Guid.NewGuid();
        var g1 = Guid.NewGuid();
        var g2 = Guid.NewGuid();
        var t0 = DateTimeOffset.UtcNow;

        var pos = new TransporterPositionDto(transporterId, 10.0, 20.0, t0);

        _geofenceEventReaderMock.Setup(r => r.GetOpenEventsForTransporterAsync(transporterId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _geofenceReaderMock.Setup(r => r.GetGeofenceIdsContainingPointAsync(It.IsAny<Guid>(), pos.Latitude, pos.Longitude, It.IsAny<CancellationToken>()))
            .ReturnsAsync([g1, g2]);

        var created1 = new GeofenceEventVm(Guid.NewGuid(), transporterId, g1, t0, null, pos.Latitude, pos.Longitude);
        var created2 = new GeofenceEventVm(Guid.NewGuid(), transporterId, g2, t0, null, pos.Latitude, pos.Longitude);

        _geofenceEventWriterMock.SetupSequence(w => w.CreateEntryEventAsync(It.IsAny<GeofenceEventDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created1)
            .ReturnsAsync(created2);

        var service = CreateService();

        // Act
        var result = await service.ProcessPositionsAsync([pos], Guid.NewGuid(), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.ProcessedCount, Is.EqualTo(1));
            Assert.That(result.EventsCreated, Is.EqualTo(2));
            Assert.That(result.EventsUpdated, Is.Zero);
        }
        _geofenceEventWriterMock.Verify(w => w.CreateEntryEventAsync(It.IsAny<GeofenceEventDto>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task ProcessPositionsAsync_Entry_AlertOnEntry_EmitsEnteredOnce()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var geofenceId = Guid.NewGuid();
        var t0 = DateTimeOffset.UtcNow;
        var position = new TransporterPositionDto(transporterId, 10.0, 20.0, t0);

        _geofenceEventReaderMock.Setup(r => r.GetOpenEventsForTransporterAsync(transporterId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _geofenceReaderMock.Setup(r => r.GetGeofenceIdsContainingPointAsync(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([geofenceId]);

        var createdEventId = Guid.NewGuid();
        var createdEvent = new GeofenceEventVm(createdEventId, transporterId, geofenceId, t0, null, position.Latitude, position.Longitude);
        _geofenceEventWriterMock.Setup(w => w.CreateEntryEventAsync(It.IsAny<GeofenceEventDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEvent);

        _geofenceReaderMock.Setup(r => r.GetActiveGeofenceAlertInfoAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, GeofenceAlertInfoVm>
            {
                [geofenceId] = new GeofenceAlertInfoVm(geofenceId, "Zone", 1, AlertOnEntry: true, AlertOnExit: false),
            });

        var service = CreateService();

        // Act
        await service.ProcessPositionsAsync([position], accountId, CancellationToken.None);

        // Assert
        _alertEmitterMock.Verify(e => e.EmitGeofenceEnteredAsync(
            It.Is<GeofenceAlertDto>(a => a.GeofenceEventId == createdEventId && a.GeofenceId == geofenceId), It.IsAny<CancellationToken>()), Times.Once);
        _alertEmitterMock.Verify(e => e.EmitGeofenceExitedAsync(It.IsAny<GeofenceAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ProcessPositionsAsync_Entry_AlertOnEntryFalse_DoesNotEmit()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var geofenceId = Guid.NewGuid();
        var t0 = DateTimeOffset.UtcNow;
        var position = new TransporterPositionDto(transporterId, 10.0, 20.0, t0);

        _geofenceEventReaderMock.Setup(r => r.GetOpenEventsForTransporterAsync(transporterId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _geofenceReaderMock.Setup(r => r.GetGeofenceIdsContainingPointAsync(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([geofenceId]);

        var createdEvent = new GeofenceEventVm(Guid.NewGuid(), transporterId, geofenceId, t0, null, position.Latitude, position.Longitude);
        _geofenceEventWriterMock.Setup(w => w.CreateEntryEventAsync(It.IsAny<GeofenceEventDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEvent);

        _geofenceReaderMock.Setup(r => r.GetActiveGeofenceAlertInfoAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, GeofenceAlertInfoVm>
            {
                [geofenceId] = new GeofenceAlertInfoVm(geofenceId, "Zone", 1, AlertOnEntry: false, AlertOnExit: false),
            });

        var service = CreateService();

        // Act
        await service.ProcessPositionsAsync([position], accountId, CancellationToken.None);

        // Assert
        _alertEmitterMock.Verify(e => e.EmitGeofenceEnteredAsync(It.IsAny<GeofenceAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ProcessPositionsAsync_Exit_AlertOnExit_EmitsExitedWithDwellSeconds()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var geofenceId = Guid.NewGuid();
        var t0 = DateTimeOffset.UtcNow;
        var t1 = t0.AddSeconds(60);

        var posEntry = new TransporterPositionDto(transporterId, 10.0, 20.0, t0);
        var posExit = new TransporterPositionDto(transporterId, 11.0, 21.0, t1);

        _geofenceEventReaderMock.Setup(r => r.GetOpenEventsForTransporterAsync(transporterId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _geofenceReaderMock.SetupSequence(r => r.GetGeofenceIdsContainingPointAsync(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([geofenceId])
            .ReturnsAsync([]);

        var createdEventId = Guid.NewGuid();
        var createdEvent = new GeofenceEventVm(createdEventId, transporterId, geofenceId, t0, null, posEntry.Latitude, posEntry.Longitude);
        _geofenceEventWriterMock.Setup(w => w.CreateEntryEventAsync(It.IsAny<GeofenceEventDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEvent);

        var closedEvent = new GeofenceEventVm(createdEventId, transporterId, geofenceId, t0, t1, posEntry.Latitude, posEntry.Longitude);
        _geofenceEventWriterMock.Setup(w => w.UpdateExitEventAsync(createdEventId, t1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(closedEvent);

        _geofenceReaderMock.Setup(r => r.GetActiveGeofenceAlertInfoAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, GeofenceAlertInfoVm>
            {
                [geofenceId] = new GeofenceAlertInfoVm(geofenceId, "Zone", 1, AlertOnEntry: false, AlertOnExit: true),
            });

        var service = CreateService();

        // Act
        await service.ProcessPositionsAsync([posEntry, posExit], accountId, CancellationToken.None);

        // Assert
        _alertEmitterMock.Verify(e => e.EmitGeofenceExitedAsync(
            It.Is<GeofenceAlertDto>(a => a.GeofenceEventId == createdEventId && a.DwellSeconds == 60L), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ProcessPositionsAsync_EntryDto_CarriesBatchAccountId()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var geofenceId = Guid.NewGuid();
        var t0 = DateTimeOffset.UtcNow;
        var position = new TransporterPositionDto(transporterId, 10.0, 20.0, t0);
        GeofenceEventDto? receivedDto = null;

        _geofenceEventReaderMock.Setup(r => r.GetOpenEventsForTransporterAsync(transporterId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _geofenceReaderMock.Setup(r => r.GetGeofenceIdsContainingPointAsync(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([geofenceId]);
        _geofenceEventWriterMock.Setup(w => w.CreateEntryEventAsync(It.IsAny<GeofenceEventDto>(), It.IsAny<CancellationToken>()))
            .Callback<GeofenceEventDto, CancellationToken>((dto, _) => receivedDto = dto)
            .ReturnsAsync(new GeofenceEventVm(Guid.NewGuid(), transporterId, geofenceId, t0, null, position.Latitude, position.Longitude));

        var service = CreateService();

        // Act
        await service.ProcessPositionsAsync([position], accountId, CancellationToken.None);

        // Assert
        Assert.That(receivedDto, Is.Not.Null);
        Assert.That(receivedDto!.Value.AccountId, Is.EqualTo(accountId), "the entry event must carry the batch's AccountId (AC 8)");
    }

    [Test]
    public async Task ProcessPositionsAsync_RedeliveredCompletedVisit_IsNotDuplicated()
    {
        // Arrange: the writer reports the entry as a duplicate (visit already recorded and
        // completed by a previous delivery of the same batch).
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var geofenceId = Guid.NewGuid();
        var t0 = DateTimeOffset.UtcNow;
        var t1 = t0.AddSeconds(60);

        var posEntry = new TransporterPositionDto(transporterId, 10.0, 20.0, t0);
        var posExit = new TransporterPositionDto(transporterId, 11.0, 21.0, t1);

        _geofenceEventReaderMock.Setup(r => r.GetOpenEventsForTransporterAsync(transporterId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _geofenceReaderMock.SetupSequence(r => r.GetGeofenceIdsContainingPointAsync(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([geofenceId])
            .ReturnsAsync([]);
        _geofenceEventWriterMock.Setup(w => w.CreateEntryEventAsync(It.IsAny<GeofenceEventDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeofenceEventVm?)null);
        _geofenceReaderMock.Setup(r => r.GetActiveGeofenceAlertInfoAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, GeofenceAlertInfoVm>
            {
                [geofenceId] = new GeofenceAlertInfoVm(geofenceId, "Zone", 1, AlertOnEntry: true, AlertOnExit: true),
            });

        var service = CreateService();

        // Act
        var result = await service.ProcessPositionsAsync([posEntry, posExit], accountId, CancellationToken.None);

        // Assert: nothing created, nothing closed, nothing emitted (AC 3).
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.EventsCreated, Is.Zero);
            Assert.That(result.EventsUpdated, Is.Zero);
        }
        _geofenceEventWriterMock.Verify(w => w.UpdateExitEventAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        _alertEmitterMock.Verify(e => e.EmitGeofenceEnteredAsync(It.IsAny<GeofenceAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
        _alertEmitterMock.Verify(e => e.EmitGeofenceExitedAsync(It.IsAny<GeofenceAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ProcessPositionsAsync_EmitterThrows_DoesNotFail_AndCountsCorrect()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var geofenceId = Guid.NewGuid();
        var t0 = DateTimeOffset.UtcNow;
        var position = new TransporterPositionDto(transporterId, 10.0, 20.0, t0);

        _geofenceEventReaderMock.Setup(r => r.GetOpenEventsForTransporterAsync(transporterId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _geofenceReaderMock.Setup(r => r.GetGeofenceIdsContainingPointAsync(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([geofenceId]);

        var createdEvent = new GeofenceEventVm(Guid.NewGuid(), transporterId, geofenceId, t0, null, position.Latitude, position.Longitude);
        _geofenceEventWriterMock.Setup(w => w.CreateEntryEventAsync(It.IsAny<GeofenceEventDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEvent);

        _geofenceReaderMock.Setup(r => r.GetActiveGeofenceAlertInfoAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, GeofenceAlertInfoVm>
            {
                [geofenceId] = new GeofenceAlertInfoVm(geofenceId, "Zone", 1, AlertOnEntry: true, AlertOnExit: false),
            });
        _alertEmitterMock.Setup(e => e.EmitGeofenceEnteredAsync(It.IsAny<GeofenceAlertDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("emit boom"));

        var service = CreateService();

        // Act
        var result = await service.ProcessPositionsAsync([position], accountId, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ProcessedCount, Is.EqualTo(1));
            Assert.That(result.EventsCreated, Is.EqualTo(1));
            Assert.That(result.EventsUpdated, Is.Zero);
        }
        _alertEmitterMock.Verify(e => e.EmitGeofenceEnteredAsync(It.IsAny<GeofenceAlertDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
