// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using Microsoft.Extensions.Logging;
using TrackHub.Geofencing.Application.GeofenceEvents.Services;
using TrackHub.Geofencing.Domain.Interfaces;
using TrackHub.Geofencing.Domain.Records;

namespace TrackHub.Geofencing.Application.UnitTests.GeofenceEvents.Services;

[TestFixture]
public class DwellEvaluationServiceTests
{
    private Mock<IGeofenceEventReader> _geofenceEventReaderMock = null!;
    private Mock<IGeofenceEventWriter> _geofenceEventWriterMock = null!;
    private Mock<IAlertEmitter> _alertEmitterMock = null!;
    private Mock<IBackgroundJobRunRecorder> _jobRunRecorderMock = null!;

    [SetUp]
    public void SetUp()
    {
        // Create fresh mocks per test to ensure isolation
        _geofenceEventReaderMock = new Mock<IGeofenceEventReader>();
        _geofenceEventWriterMock = new Mock<IGeofenceEventWriter>();
        _alertEmitterMock = new Mock<IAlertEmitter>();
        _jobRunRecorderMock = new Mock<IBackgroundJobRunRecorder>();
    }

    private DwellEvaluationService CreateService() => new(
        _geofenceEventReaderMock.Object,
        _geofenceEventWriterMock.Object,
        _alertEmitterMock.Object,
        _jobRunRecorderMock.Object,
        Mock.Of<ILogger<DwellEvaluationService>>());

    private static DwellAlertCandidateVm Candidate(DateTimeOffset eventDateTime, int thresholdMinutes)
        => new(
            GeofenceEventId: Guid.NewGuid(),
            AccountId: Guid.NewGuid(),
            TransporterId: Guid.NewGuid(),
            GeofenceId: Guid.NewGuid(),
            GeofenceName: "Zone",
            GeofenceType: 1,
            EventDateTime: eventDateTime,
            Latitude: 10.0,
            Longitude: 20.0,
            DwellThresholdMinutes: thresholdMinutes);

    [Test]
    public async Task EvaluateDwellAsync_CandidatePastThreshold_EmitsStampsAndRecordsRun()
    {
        // Arrange
        var candidate = Candidate(DateTimeOffset.UtcNow.AddMinutes(-120), thresholdMinutes: 60);
        _geofenceEventReaderMock.Setup(r => r.GetDwellAlertCandidatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([candidate]);

        var service = CreateService();

        // Act
        var alerted = await service.EvaluateDwellAsync(CancellationToken.None);

        // Assert
        Assert.That(alerted, Is.EqualTo(1));
        _alertEmitterMock.Verify(e => e.EmitGeofenceDwellExceededAsync(
            It.Is<GeofenceAlertDto>(a => a.GeofenceEventId == candidate.GeofenceEventId), It.IsAny<CancellationToken>()), Times.Once);
        _geofenceEventWriterMock.Verify(w => w.StampDwellAlertedAsync(candidate.GeofenceEventId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
        _jobRunRecorderMock.Verify(r => r.RecordAsync(
            DwellEvaluationService.JobKey, It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task EvaluateDwellAsync_CandidateNotYetPastThreshold_DoesNothing()
    {
        // Arrange
        var candidate = Candidate(DateTimeOffset.UtcNow, thresholdMinutes: 60);
        _geofenceEventReaderMock.Setup(r => r.GetDwellAlertCandidatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([candidate]);

        var service = CreateService();

        // Act
        var alerted = await service.EvaluateDwellAsync(CancellationToken.None);

        // Assert
        Assert.That(alerted, Is.Zero);
        _alertEmitterMock.Verify(e => e.EmitGeofenceDwellExceededAsync(It.IsAny<GeofenceAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
        _geofenceEventWriterMock.Verify(w => w.StampDwellAlertedAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        _jobRunRecorderMock.Verify(r => r.RecordAsync(
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ----- DwellThresholdMinutes: both sides of the boundary -------------------------------------
    //
    // The guard is `EventDateTime + threshold > now -> skip`, so the threshold is inclusive: a visit
    // whose elapsed time has just reached it alerts. Cases two hours away from the line pass even
    // with the comparison deleted; these sit one minute either side of it.

    [Test]
    public async Task EvaluateDwellAsync_OneMinuteShortOfTheThreshold_DoesNotEmit()
    {
        // Arrange
        var candidate = Candidate(DateTimeOffset.UtcNow.AddMinutes(-59), thresholdMinutes: 60);
        _geofenceEventReaderMock.Setup(r => r.GetDwellAlertCandidatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([candidate]);

        // Act
        var alerted = await CreateService().EvaluateDwellAsync(CancellationToken.None);

        // Assert
        Assert.That(alerted, Is.Zero);
        _alertEmitterMock.Verify(e => e.EmitGeofenceDwellExceededAsync(
            It.IsAny<GeofenceAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task EvaluateDwellAsync_ExactlyAtTheThreshold_Emits()
    {
        // Arrange
        var candidate = Candidate(DateTimeOffset.UtcNow.AddMinutes(-60), thresholdMinutes: 60);
        _geofenceEventReaderMock.Setup(r => r.GetDwellAlertCandidatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([candidate]);

        // Act
        var alerted = await CreateService().EvaluateDwellAsync(CancellationToken.None);

        // Assert
        Assert.That(alerted, Is.EqualTo(1));
        _alertEmitterMock.Verify(e => e.EmitGeofenceDwellExceededAsync(
            It.IsAny<GeofenceAlertDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task EvaluateDwellAsync_OneMinutePastTheThreshold_Emits()
    {
        // Arrange
        var candidate = Candidate(DateTimeOffset.UtcNow.AddMinutes(-61), thresholdMinutes: 60);
        _geofenceEventReaderMock.Setup(r => r.GetDwellAlertCandidatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([candidate]);

        // Act
        var alerted = await CreateService().EvaluateDwellAsync(CancellationToken.None);

        // Assert
        Assert.That(alerted, Is.EqualTo(1));
    }

    [Test]
    public async Task EvaluateDwellAsync_HonoursEachCandidatesOwnThreshold_WhenItIsWider()
    {
        // Two hours parked against a four-hour tolerance is not an alert. Against the 60 minutes
        // every other test uses it would be, so a service that ignores DwellThresholdMinutes - or
        // hard-codes one - fails here.
        var candidate = Candidate(DateTimeOffset.UtcNow.AddMinutes(-120), thresholdMinutes: 240);
        _geofenceEventReaderMock.Setup(r => r.GetDwellAlertCandidatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([candidate]);

        var alerted = await CreateService().EvaluateDwellAsync(CancellationToken.None);

        Assert.That(alerted, Is.Zero);
        _alertEmitterMock.Verify(e => e.EmitGeofenceDwellExceededAsync(
            It.IsAny<GeofenceAlertDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task EvaluateDwellAsync_HonoursEachCandidatesOwnThreshold_WhenItIsTighter()
    {
        // Five minutes parked against a five-minute tolerance alerts, though 60 minutes would not.
        var candidate = Candidate(DateTimeOffset.UtcNow.AddMinutes(-5), thresholdMinutes: 5);
        _geofenceEventReaderMock.Setup(r => r.GetDwellAlertCandidatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([candidate]);

        var alerted = await CreateService().EvaluateDwellAsync(CancellationToken.None);

        Assert.That(alerted, Is.EqualTo(1));
    }

    [Test]
    public async Task EvaluateDwellAsync_EvaluatesEachCandidateAgainstItsOwnThreshold()
    {
        // A mixed batch: only the candidate whose own elapsed time reaches its own threshold may
        // alert. A single shared comparison would let both through or hold both back.
        var due = Candidate(DateTimeOffset.UtcNow.AddMinutes(-30), thresholdMinutes: 20);
        var notDue = Candidate(DateTimeOffset.UtcNow.AddMinutes(-30), thresholdMinutes: 45);
        _geofenceEventReaderMock.Setup(r => r.GetDwellAlertCandidatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([due, notDue]);

        var alerted = await CreateService().EvaluateDwellAsync(CancellationToken.None);

        Assert.That(alerted, Is.EqualTo(1));
        _alertEmitterMock.Verify(e => e.EmitGeofenceDwellExceededAsync(
            It.Is<GeofenceAlertDto>(a => a.GeofenceEventId == due.GeofenceEventId), It.IsAny<CancellationToken>()), Times.Once);
        _alertEmitterMock.Verify(e => e.EmitGeofenceDwellExceededAsync(
            It.Is<GeofenceAlertDto>(a => a.GeofenceEventId == notDue.GeofenceEventId), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task EvaluateDwellAsync_EmitterThrows_DoesNotStamp_ReturnsZero_NoRun()
    {
        // Arrange
        var candidate = Candidate(DateTimeOffset.UtcNow.AddMinutes(-120), thresholdMinutes: 60);
        _geofenceEventReaderMock.Setup(r => r.GetDwellAlertCandidatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([candidate]);
        _alertEmitterMock.Setup(e => e.EmitGeofenceDwellExceededAsync(It.IsAny<GeofenceAlertDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("emit boom"));

        var service = CreateService();

        // Act
        var alerted = await service.EvaluateDwellAsync(CancellationToken.None);

        // Assert
        Assert.That(alerted, Is.Zero);
        _geofenceEventWriterMock.Verify(w => w.StampDwellAlertedAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        _jobRunRecorderMock.Verify(r => r.RecordAsync(
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task EvaluateDwellAsync_NoCandidates_DoesNotRecordRun()
    {
        // Arrange
        _geofenceEventReaderMock.Setup(r => r.GetDwellAlertCandidatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var service = CreateService();

        // Act
        var alerted = await service.EvaluateDwellAsync(CancellationToken.None);

        // Assert
        Assert.That(alerted, Is.Zero);
        _jobRunRecorderMock.Verify(r => r.RecordAsync(
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
