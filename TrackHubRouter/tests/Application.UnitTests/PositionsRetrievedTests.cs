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

using Application.UnitTests;
using Microsoft.Extensions.Logging;
using Moq;
using TrackHub.Router.Application.DevicePositions.Events;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Application.UnitTests.DevicePositions.Events;

[TestFixture]
public class PositionsRetrievedTests : TestsContext
{
    private static PositionsRetrieved.Notification.EventHandler CreateHandler(
        Mock<TrackHub.Router.Domain.Interfaces.Manager.IPositionWriter> positionWriterMock,
        Mock<TrackHub.Router.Domain.Interfaces.Geofence.IGeofenceWriter> geofenceWriterMock,
        Mock<TrackHub.Router.Domain.Interfaces.Manager.IOperatorSyncRunWriter> syncRunMock,
        Mock<TrackHub.Router.Domain.Interfaces.Manager.IAlertEventWriter> alertMock,
        Mock<TrackHub.Router.Domain.Interfaces.Trip.ITripPositionWriter>? tripWriterMock = null)
    {
        // Enrichment disabled in tests: budget 0 keeps the geocoder out of the write path.
        var geocodingMock = new Mock<TrackHub.Router.Domain.Interfaces.Geocoding.IReverseGeocodingService>();
        geocodingMock.Setup(x => x.GetEnrichmentBudgetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
        var historyMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IPositionHistorySystemWriter>();
        return new(positionWriterMock.Object, geofenceWriterMock.Object,
            (tripWriterMock ?? new Mock<TrackHub.Router.Domain.Interfaces.Trip.ITripPositionWriter>()).Object,
            syncRunMock.Object, alertMock.Object,
            geocodingMock.Object,
            historyMock.Object,
            Mock.Of<ILogger<PositionsRetrieved.Notification.EventHandler>>());
    }

    private static PositionsRetrieved.Notification BuildNotification(IEnumerable<PositionVm> positions, AccountSettingsVm account)
    {
        var op = new OperatorVm(Guid.NewGuid(), 1, account.AccountId, null);
        return new PositionsRetrieved.Notification(positions, account, op, DateTimeOffset.UtcNow, "AUTOMATIC", Guid.NewGuid().ToString());
    }

    [Test]
    public async Task EventHandler_CallsPositionWriterAndGeofenceWhenEnabled_AndRecordsSucceededRun()
    {
        var positionWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IPositionWriter>();
        var geofenceWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Geofence.IGeofenceWriter>();
        var syncRunMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IOperatorSyncRunWriter>();
        var alertMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IAlertEventWriter>();

        var handler = CreateHandler(positionWriterMock, geofenceWriterMock, syncRunMock, alertMock);

        var positions = new[] { new PositionVm { TransporterId = Guid.NewGuid(), DeviceDateTime = DateTimeOffset.UtcNow, Latitude = 0, Longitude = 0 } };
        var account = new AccountSettingsVm(Guid.NewGuid(), 10, true, true);
        var notification = BuildNotification(positions, account);

        positionWriterMock.Setup(x => x.AddOrUpdatePositionAsync(It.IsAny<IEnumerable<PositionVm>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        geofenceWriterMock.Setup(x => x.ProcessPositionsAsync(It.IsAny<IEnumerable<PositionVm>>(), account.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TrackHub.Router.Domain.Models.GeofenceProcessingResultVm(0, 1, 0));

        await handler.Handle(notification, CancellationToken.None);

        positionWriterMock.Verify(x => x.AddOrUpdatePositionAsync(
            It.Is<IEnumerable<PositionVm>>(p => p.Single().TransporterId == positions[0].TransporterId),
            It.IsAny<CancellationToken>()), Times.Once);
        geofenceWriterMock.Verify(x => x.ProcessPositionsAsync(It.IsAny<IEnumerable<PositionVm>>(), account.AccountId, It.IsAny<CancellationToken>()), Times.Once);
        syncRunMock.Verify(x => x.RecordAsync(It.Is<OperatorSyncRunDto>(d => d.Result == "SUCCEEDED" && d.PositionsAccepted == 1), It.IsAny<CancellationToken>()), Times.Once);
        alertMock.Verify(x => x.RecordAsync(It.IsAny<AlertEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task EventHandler_RecordsFailedRunAndAlert_WhenPositionWriterThrows()
    {
        var positionWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IPositionWriter>();
        var geofenceWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Geofence.IGeofenceWriter>();
        var syncRunMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IOperatorSyncRunWriter>();
        var alertMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IAlertEventWriter>();

        var handler = CreateHandler(positionWriterMock, geofenceWriterMock, syncRunMock, alertMock);

        var positions = new[] { new PositionVm { TransporterId = Guid.NewGuid(), DeviceDateTime = DateTimeOffset.UtcNow, Latitude = 0, Longitude = 0 } };
        var account = new AccountSettingsVm(Guid.NewGuid(), 10, true, true);
        var notification = BuildNotification(positions, account);

        positionWriterMock.Setup(x => x.AddOrUpdatePositionAsync(It.IsAny<IEnumerable<PositionVm>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        Assert.DoesNotThrowAsync(async () => await handler.Handle(notification, CancellationToken.None));

        syncRunMock.Verify(x => x.RecordAsync(It.Is<OperatorSyncRunDto>(d => d.Result == "FAILED" && d.ErrorCode == "InvalidOperationException"), It.IsAny<CancellationToken>()), Times.Once);
        alertMock.Verify(x => x.RecordAsync(It.Is<AlertEventDto>(a => a.EventType == "GpsOperatorPositionSyncFailed"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task EventHandler_RecordsFailedRunAndAlert_WhenProviderErrorProvided()
    {
        // Regression for router-audit A-08: a provider position-fetch that threw is carried on the
        // notification as ProviderErrorCode. It must be recorded as a FAILED run (not a silent
        // SUCCEEDED/read=0) and raise the sync-failed alert, even with zero positions.
        var positionWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IPositionWriter>();
        var geofenceWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Geofence.IGeofenceWriter>();
        var syncRunMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IOperatorSyncRunWriter>();
        var alertMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IAlertEventWriter>();

        var handler = CreateHandler(positionWriterMock, geofenceWriterMock, syncRunMock, alertMock);
        var account = new AccountSettingsVm(Guid.NewGuid(), 10, true, true);
        var op = new OperatorVm(Guid.NewGuid(), 1, account.AccountId, null);
        var notification = new PositionsRetrieved.Notification(
            [],
            account,
            op,
            DateTimeOffset.UtcNow,
            "AUTOMATIC",
            Guid.NewGuid().ToString(),
            ProviderErrorCode: "HttpRequestException",
            ProviderErrorMessage: "provider unreachable");

        Assert.DoesNotThrowAsync(async () => await handler.Handle(notification, CancellationToken.None));

        syncRunMock.Verify(x => x.RecordAsync(It.Is<OperatorSyncRunDto>(d => d.Result == "FAILED" && d.ErrorCode == "HttpRequestException"), It.IsAny<CancellationToken>()), Times.Once);
        alertMock.Verify(x => x.RecordAsync(It.Is<AlertEventDto>(a => a.EventType == "GpsOperatorPositionSyncFailed"), It.IsAny<CancellationToken>()), Times.Once);
        positionWriterMock.Verify(x => x.AddOrUpdatePositionAsync(It.IsAny<IEnumerable<PositionVm>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task EventHandler_GeofenceFailureDoesNotFailRun_WhenPositionsStored()
    {
        // Regression for router-audit A-09: a Geofencing outage after a successful position write
        // must not flip the run to FAILED nor raise a false alert — positions were stored.
        var positionWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IPositionWriter>();
        var geofenceWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Geofence.IGeofenceWriter>();
        var syncRunMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IOperatorSyncRunWriter>();
        var alertMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IAlertEventWriter>();

        var handler = CreateHandler(positionWriterMock, geofenceWriterMock, syncRunMock, alertMock);

        var positions = new[] { new PositionVm { TransporterId = Guid.NewGuid(), DeviceDateTime = DateTimeOffset.UtcNow, Latitude = 0, Longitude = 0 } };
        var account = new AccountSettingsVm(Guid.NewGuid(), 10, true, true);
        var notification = BuildNotification(positions, account);

        positionWriterMock.Setup(x => x.AddOrUpdatePositionAsync(It.IsAny<IEnumerable<PositionVm>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        geofenceWriterMock.Setup(x => x.ProcessPositionsAsync(It.IsAny<IEnumerable<PositionVm>>(), account.AccountId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("geofencing down"));

        Assert.DoesNotThrowAsync(async () => await handler.Handle(notification, CancellationToken.None));

        syncRunMock.Verify(x => x.RecordAsync(It.Is<OperatorSyncRunDto>(d => d.Result == "SUCCEEDED" && d.PositionsAccepted == 1), It.IsAny<CancellationToken>()), Times.Once);
        alertMock.Verify(x => x.RecordAsync(It.IsAny<AlertEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task EventHandler_CallsTripWriter_WhenTripManagementEnabled()
    {
        // Spec 11 §15: the trip position feed runs right after the geofence feed, gated by the
        // already-populated AccountSettingsVm.TripManagementEnabled.
        var positionWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IPositionWriter>();
        var geofenceWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Geofence.IGeofenceWriter>();
        var syncRunMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IOperatorSyncRunWriter>();
        var alertMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IAlertEventWriter>();
        var tripWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Trip.ITripPositionWriter>();

        var handler = CreateHandler(positionWriterMock, geofenceWriterMock, syncRunMock, alertMock, tripWriterMock);

        var positions = new[] { new PositionVm { TransporterId = Guid.NewGuid(), DeviceDateTime = DateTimeOffset.UtcNow, Latitude = 0, Longitude = 0 } };
        var account = new AccountSettingsVm(Guid.NewGuid(), 10, GeofencingEnabled: false, TripManagementEnabled: true);
        var notification = BuildNotification(positions, account);

        positionWriterMock.Setup(x => x.AddOrUpdatePositionAsync(It.IsAny<IEnumerable<PositionVm>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        tripWriterMock.Setup(x => x.ProcessTripPositionsAsync(It.IsAny<IEnumerable<PositionVm>>(), account.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TripProcessingResultVm(1, 1, 0, 0));

        await handler.Handle(notification, CancellationToken.None);

        tripWriterMock.Verify(x => x.ProcessTripPositionsAsync(It.IsAny<IEnumerable<PositionVm>>(), account.AccountId, It.IsAny<CancellationToken>()), Times.Once);
        syncRunMock.Verify(x => x.RecordAsync(It.Is<OperatorSyncRunDto>(d => d.Result == "SUCCEEDED" && d.PositionsAccepted == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task EventHandler_DoesNotCallTripWriter_WhenTripManagementDisabled()
    {
        var positionWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IPositionWriter>();
        var geofenceWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Geofence.IGeofenceWriter>();
        var syncRunMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IOperatorSyncRunWriter>();
        var alertMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IAlertEventWriter>();
        var tripWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Trip.ITripPositionWriter>();

        var handler = CreateHandler(positionWriterMock, geofenceWriterMock, syncRunMock, alertMock, tripWriterMock);

        var positions = new[] { new PositionVm { TransporterId = Guid.NewGuid(), DeviceDateTime = DateTimeOffset.UtcNow, Latitude = 0, Longitude = 0 } };
        var account = new AccountSettingsVm(Guid.NewGuid(), 10, GeofencingEnabled: true, TripManagementEnabled: false);
        var notification = BuildNotification(positions, account);

        positionWriterMock.Setup(x => x.AddOrUpdatePositionAsync(It.IsAny<IEnumerable<PositionVm>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await handler.Handle(notification, CancellationToken.None);

        tripWriterMock.Verify(x => x.ProcessTripPositionsAsync(It.IsAny<IEnumerable<PositionVm>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        geofenceWriterMock.Verify(x => x.ProcessPositionsAsync(It.IsAny<IEnumerable<PositionVm>>(), account.AccountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task EventHandler_TripFeedFailureDoesNotFailRun_WhenPositionsStored()
    {
        // Mirrors the geofence isolation guarantee (router-audit A-09): a TripManagement outage
        // after a successful position write must not flip the run to FAILED nor raise an alert.
        var positionWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IPositionWriter>();
        var geofenceWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Geofence.IGeofenceWriter>();
        var syncRunMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IOperatorSyncRunWriter>();
        var alertMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IAlertEventWriter>();
        var tripWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Trip.ITripPositionWriter>();

        var handler = CreateHandler(positionWriterMock, geofenceWriterMock, syncRunMock, alertMock, tripWriterMock);

        var positions = new[] { new PositionVm { TransporterId = Guid.NewGuid(), DeviceDateTime = DateTimeOffset.UtcNow, Latitude = 0, Longitude = 0 } };
        var account = new AccountSettingsVm(Guid.NewGuid(), 10, GeofencingEnabled: false, TripManagementEnabled: true);
        var notification = BuildNotification(positions, account);

        positionWriterMock.Setup(x => x.AddOrUpdatePositionAsync(It.IsAny<IEnumerable<PositionVm>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        tripWriterMock.Setup(x => x.ProcessTripPositionsAsync(It.IsAny<IEnumerable<PositionVm>>(), account.AccountId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("trip management down"));

        Assert.DoesNotThrowAsync(async () => await handler.Handle(notification, CancellationToken.None));

        syncRunMock.Verify(x => x.RecordAsync(It.Is<OperatorSyncRunDto>(d => d.Result == "SUCCEEDED" && d.PositionsAccepted == 1), It.IsAny<CancellationToken>()), Times.Once);
        alertMock.Verify(x => x.RecordAsync(It.IsAny<AlertEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task EventHandler_WritesFreshProjectionFirstThenReUpsertsEnrichedAddresses()
    {
        // Regression for router-audit A-10: the fresh projection is written BEFORE reverse-geocoding
        // (so geocoding's fleet-wide throttle cannot delay position storage); enrichment then
        // resolves the blank address and re-upserts only the changed projection.
        var positionWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IPositionWriter>();
        positionWriterMock.Setup(x => x.AddOrUpdatePositionAsync(It.IsAny<IEnumerable<PositionVm>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var geofenceWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Geofence.IGeofenceWriter>();
        var syncRunMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IOperatorSyncRunWriter>();
        var alertMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IAlertEventWriter>();
        var historyMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IPositionHistorySystemWriter>();

        var geocodingMock = new Mock<TrackHub.Router.Domain.Interfaces.Geocoding.IReverseGeocodingService>();
        geocodingMock.Setup(x => x.GetEnrichmentBudgetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(5);
        geocodingMock.Setup(x => x.TryResolveAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddressVm("Resolved St 123", "Bogota", "DC", "CO"));

        var handler = new PositionsRetrieved.Notification.EventHandler(
            positionWriterMock.Object, geofenceWriterMock.Object,
            new Mock<TrackHub.Router.Domain.Interfaces.Trip.ITripPositionWriter>().Object,
            syncRunMock.Object, alertMock.Object,
            geocodingMock.Object, historyMock.Object,
            Mock.Of<ILogger<PositionsRetrieved.Notification.EventHandler>>());

        // Provider supplied NO address → enrichment resolves it.
        var positions = new[] { new PositionVm { TransporterId = Guid.NewGuid(), DeviceDateTime = DateTimeOffset.UtcNow, Latitude = 1, Longitude = 1, Address = null } };
        var account = new AccountSettingsVm(Guid.NewGuid(), 10, false, false);
        var notification = BuildNotification(positions, account);

        await handler.Handle(notification, CancellationToken.None);

        // Phase 1 (fresh) + Phase 2 (enriched re-upsert) = two writes.
        positionWriterMock.Verify(x => x.AddOrUpdatePositionAsync(It.IsAny<IEnumerable<PositionVm>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        // The re-upsert carried the resolved address.
        positionWriterMock.Verify(x => x.AddOrUpdatePositionAsync(
            It.Is<IEnumerable<PositionVm>>(ps => ps.Any(p => p.Address == "Resolved St 123")),
            It.IsAny<CancellationToken>()), Times.Once);
        // Still a clean SUCCEEDED run.
        syncRunMock.Verify(x => x.RecordAsync(It.Is<OperatorSyncRunDto>(d => d.Result == "SUCCEEDED" && d.PositionsAccepted == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task EventHandler_RecordsRequestedTriggerType()
    {
        var positionWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IPositionWriter>();
        var geofenceWriterMock = new Mock<TrackHub.Router.Domain.Interfaces.Geofence.IGeofenceWriter>();
        var syncRunMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IOperatorSyncRunWriter>();
        var alertMock = new Mock<TrackHub.Router.Domain.Interfaces.Manager.IAlertEventWriter>();

        var handler = CreateHandler(positionWriterMock, geofenceWriterMock, syncRunMock, alertMock);
        var account = new AccountSettingsVm(Guid.NewGuid(), 10, false, false);
        var op = new OperatorVm(Guid.NewGuid(), 1, account.AccountId, null);
        var notification = new PositionsRetrieved.Notification(
            [],
            account,
            op,
            DateTimeOffset.UtcNow,
            "MANUAL",
            "corr-42");

        await handler.Handle(notification, CancellationToken.None);

        syncRunMock.Verify(x => x.RecordAsync(
            It.Is<OperatorSyncRunDto>(d => d.TriggerType == "MANUAL" && d.CorrelationId == "corr-42"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
