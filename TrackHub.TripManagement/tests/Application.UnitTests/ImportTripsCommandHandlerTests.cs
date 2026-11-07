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

using TrackHub.TripManagement.Application.Integration.Commands.ImportTrips;
using TrackHub.TripManagement.Application.Trips.Services.Interfaces;

namespace TrackHub.TripManagement.Application.UnitTests;

[TestFixture]
public sealed class ImportTripsCommandHandlerTests
{
    private readonly Guid _serviceOrderId = Guid.NewGuid();

    [Test]
    public async Task ReImport_UpdatesOnlyTheContractFields_AndRunsInOneTransaction()
    {
        var existing = TestFactory.Trip(TripStatuses.Created) with
        {
            ExternalReference = "EXT-1",
            StaffId = TestFactory.StaffId,
            ServiceOrderId = _serviceOrderId,
            Notes = "Dock 4",
            TollVehicleClass = "III",
        };
        var reader = new Mock<ITripReader>();
        reader.Setup(r => r.FindByExternalReferenceAsync(TestFactory.AccountId, "EXT-1", It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        reader.Setup(r => r.TransporterExistsInAccountAsync(existing.TransporterId, TestFactory.AccountId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var writer = new Mock<ITripWriter>();
        var stopWriter = new Mock<ITripStopWriter>();
        var transaction = new Mock<ITripTransaction>();
        transaction.Setup(t => t.ExecuteAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((work, token) => work(token));
        var handler = new ImportTripsCommandHandler(
            writer.Object, reader.Object, stopWriter.Object, Mock.Of<IManagerValidationClient>(),
            Mock.Of<ITripStartBackfillService>(), Mock.Of<ITransporterTollClassStore>(), transaction.Object,
            TestFactory.Logger<ImportTripsCommandHandler>());

        var item = new TripImportDto("EXT-1", existing.Code, existing.TransporterId, null, null, "Depot", 4.6, -74.1, null,
            existing.PlannedStartAt, null, null, null, [new TripStopDto("Stop", null, null, 4.7, -74.0, null, 150, null, null, null, false, 0, null)]);

        var results = await handler.Handle(new ImportTripsCommand(TestFactory.AccountId, [item]), CancellationToken.None);

        Assert.That(results.Single().Succeeded, Is.True);
        writer.Verify(w => w.UpdateTripAsync(existing.TripId, It.Is<TripDto>(d =>
            d.StaffId == TestFactory.StaffId
            && d.ServiceOrderId == _serviceOrderId
            && d.Notes == "Dock 4"
            && d.TollVehicleClass == "III"), TestFactory.AccountId, null, It.IsAny<CancellationToken>()), Times.Once);
        transaction.Verify(t => t.ExecuteAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()), Times.Once);
        stopWriter.Verify(s => s.ReplaceStopsAsync(existing.TripId, TestFactory.AccountId, It.IsAny<IReadOnlyCollection<TripStopDto>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ReImport_OntoAnotherUnit_ChecksTheCarriedOverStaff()
    {
        var existing = TestFactory.Trip(TripStatuses.Created) with { ExternalReference = "EXT-2", StaffId = TestFactory.StaffId };
        var otherUnit = Guid.NewGuid();
        var reader = new Mock<ITripReader>();
        reader.Setup(r => r.FindByExternalReferenceAsync(TestFactory.AccountId, "EXT-2", It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        reader.Setup(r => r.TransporterExistsInAccountAsync(otherUnit, TestFactory.AccountId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var validation = new Mock<IManagerValidationClient>();
        validation.Setup(v => v.ValidateStaffAssignmentAsync(TestFactory.StaffId, "Transporter", otherUnit, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var writer = new Mock<ITripWriter>();
        var handler = new ImportTripsCommandHandler(
            writer.Object, reader.Object, Mock.Of<ITripStopWriter>(), validation.Object,
            Mock.Of<ITripStartBackfillService>(), Mock.Of<ITransporterTollClassStore>(), Mock.Of<ITripTransaction>(),
            TestFactory.Logger<ImportTripsCommandHandler>());

        var item = new TripImportDto("EXT-2", existing.Code, otherUnit, null, null, "Depot", 4.6, -74.1, null,
            existing.PlannedStartAt, null, null, null, []);

        var results = await handler.Handle(new ImportTripsCommand(TestFactory.AccountId, [item]), CancellationToken.None);

        Assert.That(results.Single().Succeeded, Is.False);
        writer.Verify(w => w.UpdateTripAsync(It.IsAny<Guid>(), It.IsAny<TripDto>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
