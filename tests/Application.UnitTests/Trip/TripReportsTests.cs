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

using System.Globalization;
using System.Resources;
using Moq;
using TrackHub.Reporting.Application.Report.Factory.Trip;
using TrackHub.Reporting.Domain.Helpers;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Trip;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests.Trip;

[TestFixture]
public class TripReportsTests
{
    private static readonly DateTimeOffset Base = new(2026, 7, 20, 8, 0, 0, TimeSpan.Zero);

    private Mock<ITripReportReader> _reader = null!;
    private FilterDto _filters;

    [SetUp]
    public void SetUp()
    {
        _reader = new Mock<ITripReportReader>();
        _reader.Setup(r => r.EnsureTripManagementFeatureAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _filters = new FilterDto { Language = "en", Name = "Test" };
    }

    private static IReadOnlyCollection<T> List<T>(params T[] items) => items;

    private static ReportTripVm Trip(
        string code,
        DateTimeOffset plannedStart,
        DateTimeOffset? plannedEnd = null,
        DateTimeOffset? actualEnd = null,
        string? driverName = "Ana",
        double? plannedDistanceMeters = 12_500,
        double actualDistanceMeters = 13_000,
        decimal? toll = 42.5m,
        string? currency = "COP")
        => new(
            Guid.NewGuid(), code, "Completed", Guid.NewGuid(), "Truck 1", Guid.NewGuid(), driverName, "ACME",
            plannedStart, plannedEnd, plannedStart.AddMinutes(5), actualEnd,
            plannedDistanceMeters, actualDistanceMeters, 3, toll, currency, "Complete");

    private static ReportTripStopVm Stop(
        string tripCode,
        int sequence,
        string name,
        DateTimeOffset? plannedTo,
        DateTimeOffset? arrivedAt,
        DateTimeOffset? departedAt = null,
        string? driverName = "Ana",
        string? customerName = "ACME",
        string transporterName = "Truck 1",
        int deliveries = 2,
        int delivered = 1,
        int failed = 1,
        int partial = 0)
        => new(
            Guid.NewGuid(), Guid.NewGuid(), tripCode, transporterName, driverName, customerName,
            sequence, name, "Departed", plannedTo?.AddMinutes(-30), plannedTo, arrivedAt, departedAt,
            deliveries, delivered, failed, partial);

    [Test]
    public void Codes_MatchSpecConstants()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new TripSummaryReport(_reader.Object).ReportCode, Is.EqualTo("trip-summary"));
            Assert.That(new TripDetailReport(_reader.Object).ReportCode, Is.EqualTo("trip-detail"));
            Assert.That(new TripOnTimePerformanceReport(_reader.Object).ReportCode, Is.EqualTo("trip-on-time-performance"));
            Assert.That(new TripStopDwellReport(_reader.Object).ReportCode, Is.EqualTo("trip-stop-dwell"));
            Assert.That(new TripTollCostReport(_reader.Object).ReportCode, Is.EqualTo("trip-toll-cost"));
            Assert.That(new TripPodExportReport(_reader.Object).ReportCode, Is.EqualTo("trip-pod-export"));
        });
    }

    // ---- trip-summary ----

    [Test]
    public async Task Summary_OrdersMostRecentFirst_AndConvertsDistanceToKilometers()
    {
        _reader.Setup(r => r.GetTripsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(
                Trip("T-100", Base.AddDays(-3)),
                Trip("T-200", Base)));

        var result = await new TripSummaryReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Cell(0, "TripCode"), Is.EqualTo("T-200"));
            Assert.That(result.Cell(1, "TripCode"), Is.EqualTo("T-100"));
            Assert.That(result.Cell(0, "PlannedDistanceKm"), Is.EqualTo(12.5d));
            Assert.That(result.Cell(0, "ActualDistanceKm"), Is.EqualTo(13d));
        });
    }

    // The on-time flag is deliberately tri-state: an unfinished or unplanned trip is not a
    // punctuality failure, so it must read as blank rather than "false".
    [Test]
    public async Task Summary_OnTimeIsNullWhenEitherEndIsUnknown()
    {
        _reader.Setup(r => r.GetTripsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(
                Trip("A", Base.AddDays(3), plannedEnd: Base.AddDays(3).AddHours(4), actualEnd: Base.AddDays(3).AddHours(3)),
                Trip("B", Base.AddDays(2), plannedEnd: Base.AddDays(2).AddHours(4), actualEnd: Base.AddDays(2).AddHours(6)),
                Trip("C", Base.AddDays(1), plannedEnd: Base.AddDays(1).AddHours(4), actualEnd: null),
                Trip("D", Base, plannedEnd: null, actualEnd: Base.AddHours(2))));

        var result = await new TripSummaryReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Cell(0, "OnTime"), Is.True);
            Assert.That(result.Cell(1, "OnTime"), Is.False);
            Assert.That(result.Cell(2, "OnTime"), Is.Null);
            Assert.That(result.Cell(3, "OnTime"), Is.Null);
        });
    }

    [Test]
    public async Task Summary_NullsProjectToEmptyStringsNeverNullCells()
    {
        _reader.Setup(r => r.GetTripsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(Trip("T-1", Base, driverName: null, plannedDistanceMeters: null, toll: null, currency: null)));

        var result = await new TripSummaryReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Cell(0, "DriverName"), Is.EqualTo(string.Empty));
            Assert.That(result.Cell(0, "TollCurrency"), Is.EqualTo(string.Empty));
            // A missing plan or an unpopulated toll catalog stays NULL — never 0, which would read
            // as "no distance"/"free" (spec 11 §7.7).
            Assert.That(result.Cell(0, "PlannedDistanceKm"), Is.Null);
            Assert.That(result.Cell(0, "EstimatedTollAmount"), Is.Null);
        });
    }

    // StringFilter1 is the portal's transporter picker slot; the window comes from DateTimeFilter1/2.
    [Test]
    public async Task Summary_ReadsStringFilter1AsTransporterIdAndIgnoresGarbage()
    {
        var transporterId = Guid.NewGuid();
        _reader.Setup(r => r.GetTripsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportTripVm>());

        await new TripSummaryReport(_reader.Object).GetDatasetAsync(
            _filters with { StringFilter1 = transporterId.ToString(), DateTimeFilter1 = Base, DateTimeFilter2 = Base.AddDays(1) },
            CancellationToken.None);

        _reader.Verify(r => r.GetTripsAsync(Base, Base.AddDays(1), transporterId, null, It.IsAny<CancellationToken>()), Times.Once);

        // An unparseable or empty picker value means "no filter", never an error and never a literal.
        // StringFilter2 is the portal's free-text device slot and is not part of this report's spec.
        await new TripSummaryReport(_reader.Object).GetDatasetAsync(
            _filters with { StringFilter1 = "not-a-guid", StringFilter2 = Guid.NewGuid().ToString() }, CancellationToken.None);
        await new TripSummaryReport(_reader.Object).GetDatasetAsync(
            _filters with { StringFilter1 = Guid.Empty.ToString() }, CancellationToken.None);

        _reader.Verify(r => r.GetTripsAsync(null, null, null, null, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task Summary_ColumnOrderMatchesRowVmDeclaration()
    {
        _reader.Setup(r => r.GetTripsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportTripVm>());

        var result = await new TripSummaryReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Columns.Select(c => c.PropertyName), Is.EqualTo(new[]
        {
            "TripCode", "TripStatus", "TransporterName", "DriverName",
            "PlannedStartAt", "ActualStartAt", "PlannedEndAt", "ActualEndAt",
            "PlannedDistanceKm", "ActualDistanceKm", "StopCount", "OnTime",
            "EstimatedTollAmount", "TollCurrency"
        }));
    }

    // ---- trip-detail ----

    [Test]
    public async Task Detail_OrdersByTripThenSequence_AndComputesDwell()
    {
        _reader.Setup(r => r.GetTripStopsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(
                Stop("T-2", 1, "Warehouse", Base, Base, Base.AddMinutes(45)),
                Stop("T-1", 2, "Store B", Base, Base, Base.AddMinutes(15)),
                Stop("T-1", 1, "Store A", Base, Base, departedAt: null)));

        var result = await new TripDetailReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Column("TripCode"), Is.EqualTo(new object?[] { "T-1", "T-1", "T-2" }));
            Assert.That(result.Column("StopSequence"), Is.EqualTo(new object?[] { 1, 2, 1 }));
            // An open visit has no dwell yet — null, not zero.
            Assert.That(result.Cell(0, "DwellMinutes"), Is.Null);
            Assert.That(result.Cell(1, "DwellMinutes"), Is.EqualTo(15d));
            Assert.That(result.Cell(2, "DwellMinutes"), Is.EqualTo(45d));
        });
    }

    [Test]
    public async Task Detail_CarriesDeliveryOutcomeCounts()
    {
        _reader.Setup(r => r.GetTripStopsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(Stop("T-1", 1, "Store A", Base, Base, Base.AddMinutes(10), deliveries: 5, delivered: 3, failed: 1, partial: 1)));

        var result = await new TripDetailReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Cell(0, "DeliveryCount"), Is.EqualTo(5));
            Assert.That(result.Cell(0, "DeliveredCount"), Is.EqualTo(3));
            Assert.That(result.Cell(0, "FailedDeliveryCount"), Is.EqualTo(1));
            Assert.That(result.Cell(0, "PartialDeliveryCount"), Is.EqualTo(1));
            Assert.That(result.Cell(0, "StopStatus"), Is.EqualTo("Departed"));
        });
    }

    [Test]
    public async Task Detail_ColumnOrderMatchesRowVmDeclaration()
    {
        _reader.Setup(r => r.GetTripStopsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportTripStopVm>());

        var result = await new TripDetailReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Columns.Select(c => c.PropertyName), Is.EqualTo(new[]
        {
            "TripCode", "StopSequence", "StopName", "CustomerName",
            "PlannedArrivalFrom", "PlannedArrivalTo", "ActualArrivalAt", "ActualDepartureAt",
            "DwellMinutes", "StopStatus",
            "DeliveryCount", "DeliveredCount", "FailedDeliveryCount", "PartialDeliveryCount"
        }));
    }

    // ---- trip-on-time-performance ----

    [Test]
    public async Task OnTime_AggregatesPerTransporterDriverCustomer()
    {
        _reader.Setup(r => r.GetTripStopsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(
                Stop("T-1", 1, "A", Base, Base.AddMinutes(10)),                       // 10 late
                Stop("T-1", 2, "B", Base, Base.AddMinutes(-5)),                       // early → on time
                Stop("T-2", 1, "C", Base, Base.AddMinutes(30)),                       // 30 late
                Stop("T-3", 1, "D", Base, Base.AddMinutes(2), customerName: "Other")));

        var result = await new TripOnTimePerformanceReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(2));

        var acme = result.IndexOfRow("CustomerName", "ACME");
        Assert.Multiple(() =>
        {
            Assert.That(result.Cell(acme, "EvaluatedStopCount"), Is.EqualTo(3));
            Assert.That(result.Cell(acme, "OnTimeStopCount"), Is.EqualTo(1));
            Assert.That(result.Cell(acme, "DelayedStopCount"), Is.EqualTo(2));
            Assert.That(result.Cell(acme, "OnTimePercent"), Is.EqualTo(33.33d));
            // An early arrival contributes 0, never -5 — running early must not offset lateness.
            Assert.That(result.Cell(acme, "AverageDelayMinutes"), Is.EqualTo(13.33d));
            Assert.That(result.Cell(acme, "MaxDelayMinutes"), Is.EqualTo(30d));
        });
    }

    // A stop with no planned window, or one never arrived at, has no punctuality to measure. Counting
    // either would silently inflate (or deflate) the percentage the report exists to state.
    [Test]
    public async Task OnTime_ExcludesStopsWithNoPlannedWindowOrNoArrival()
    {
        _reader.Setup(r => r.GetTripStopsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(
                Stop("T-1", 1, "A", Base, Base),                    // evaluable
                Stop("T-1", 2, "B", null, Base.AddMinutes(90)),     // no planned window
                Stop("T-1", 3, "C", Base, null)));                  // never arrived

        var result = await new TripOnTimePerformanceReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.RowCount, Is.EqualTo(1));
            Assert.That(result.Cell(0, "EvaluatedStopCount"), Is.EqualTo(1));
            Assert.That(result.Cell(0, "OnTimePercent"), Is.EqualTo(100d));
        });
    }

    [Test]
    public async Task OnTime_MissingDriverOrCustomerGroupsUnderOneExplicitBucket()
    {
        _reader.Setup(r => r.GetTripStopsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(
                Stop("T-1", 1, "A", Base, Base, driverName: null, customerName: null),
                Stop("T-2", 1, "B", Base, Base, driverName: "", customerName: "   ")));

        var result = await new TripOnTimePerformanceReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.RowCount, Is.EqualTo(1));
            Assert.That(result.Cell(0, "DriverName"), Is.EqualTo("-"));
            Assert.That(result.Cell(0, "CustomerName"), Is.EqualTo("-"));
            Assert.That(result.Cell(0, "EvaluatedStopCount"), Is.EqualTo(2));
        });
    }

    [Test]
    public async Task OnTime_ColumnOrderMatchesRowVmDeclaration()
    {
        _reader.Setup(r => r.GetTripStopsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportTripStopVm>());

        var result = await new TripOnTimePerformanceReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Columns.Select(c => c.PropertyName), Is.EqualTo(new[]
        {
            "TransporterName", "DriverName", "CustomerName",
            "EvaluatedStopCount", "OnTimeStopCount", "DelayedStopCount",
            "OnTimePercent", "AverageDelayMinutes", "MaxDelayMinutes"
        }));
    }

    // ---- trip-stop-dwell ----

    [Test]
    public async Task Dwell_AggregatesPerStopAndCustomer_LongestAverageFirst()
    {
        _reader.Setup(r => r.GetTripStopsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(
                Stop("T-1", 1, "Store A", Base, Base, Base.AddMinutes(10)),
                Stop("T-2", 1, "Store A", Base, Base, Base.AddMinutes(30)),
                Stop("T-3", 1, "Store B", Base, Base, Base.AddMinutes(5))));

        var result = await new TripStopDwellReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Column("StopName"), Is.EqualTo(new object?[] { "Store A", "Store B" }));
            Assert.That(result.Cell(0, "VisitCount"), Is.EqualTo(2));
            Assert.That(result.Cell(0, "AverageDwellMinutes"), Is.EqualTo(20d));
            Assert.That(result.Cell(0, "MinDwellMinutes"), Is.EqualTo(10d));
            Assert.That(result.Cell(0, "MaxDwellMinutes"), Is.EqualTo(30d));
            Assert.That(result.Cell(0, "TotalDwellMinutes"), Is.EqualTo(40d));
        });
    }

    // An open visit contributes nothing; counting it as a zero-minute dwell would drag every
    // average it lands in toward zero and make the distribution meaningless.
    [Test]
    public async Task Dwell_IgnoresOpenVisits()
    {
        _reader.Setup(r => r.GetTripStopsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(
                Stop("T-1", 1, "Store A", Base, Base, Base.AddMinutes(20)),
                Stop("T-2", 1, "Store A", Base, Base, departedAt: null),
                Stop("T-3", 1, "Store A", Base, null, departedAt: null)));

        var result = await new TripStopDwellReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.RowCount, Is.EqualTo(1));
            Assert.That(result.Cell(0, "VisitCount"), Is.EqualTo(1));
            Assert.That(result.Cell(0, "AverageDwellMinutes"), Is.EqualTo(20d));
        });
    }

    [Test]
    public async Task Dwell_ColumnOrderMatchesRowVmDeclaration()
    {
        _reader.Setup(r => r.GetTripStopsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportTripStopVm>());

        var result = await new TripStopDwellReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Columns.Select(c => c.PropertyName), Is.EqualTo(new[]
        {
            "StopName", "CustomerName", "VisitCount",
            "AverageDwellMinutes", "MinDwellMinutes", "MaxDwellMinutes", "TotalDwellMinutes"
        }));
    }

    // ---- trip-toll-cost ----

    // The whole point of the report: a station with no tariff for the trip's class must arrive as a
    // flagged NULL, never as a zero that quietly nets into the total.
    [Test]
    public async Task Toll_FlagsPartialNoTariffAndKeepsTheAmountNull()
    {
        _reader.Setup(r => r.GetTripTollsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(
                new ReportTripTollVm(Guid.NewGuid(), "T-1", Guid.NewGuid(), Base, "C3",
                    Guid.NewGuid(), "Peaje Norte", "PN", "Ruta 45", "N", 12_000m, "COP", true),
                new ReportTripTollVm(Guid.NewGuid(), "T-1", null, Base, "C3",
                    Guid.NewGuid(), "Peaje Sur", null, null, null, null, null, false)));

        var result = await new TripTollCostReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        var north = result.IndexOfRow("TollStationName", "Peaje Norte");
        var south = result.IndexOfRow("TollStationName", "Peaje Sur");
        Assert.Multiple(() =>
        {
            Assert.That(result.Cell(north, "EstimatedTollAmount"), Is.EqualTo(12_000m));
            Assert.That(result.Cell(north, "PartialNoTariff"), Is.False);
            Assert.That(result.Cell(south, "EstimatedTollAmount"), Is.Null);
            Assert.That(result.Cell(south, "PartialNoTariff"), Is.True);
            // Nulls project to empty strings, never null string cells.
            Assert.That(result.Cell(south, "RoadName"), Is.EqualTo(string.Empty));
            Assert.That(result.Cell(south, "Direction"), Is.EqualTo(string.Empty));
            Assert.That(result.Cell(south, "TollCurrency"), Is.EqualTo(string.Empty));
            Assert.That(result.Cell(south, "RoutePlanId"), Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public async Task Toll_ColumnOrderMatchesRowVmDeclaration()
    {
        _reader.Setup(r => r.GetTripTollsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportTripTollVm>());

        var result = await new TripTollCostReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Columns.Select(c => c.PropertyName), Is.EqualTo(new[]
        {
            "TripCode", "RoutePlanId", "TollStationName", "RoadName", "Direction",
            "TollVehicleClass", "EstimatedTollAmount", "TollCurrency", "PartialNoTariff"
        }));
    }

    // ---- trip-pod-export ----

    [Test]
    public async Task Pod_OrdersMostRecentCaptureFirst_AndKeepsCoordinatesNullable()
    {
        _reader.Setup(r => r.GetTripProofsOfDeliveryAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(
                new ReportTripPodVm(Guid.NewGuid(), Guid.NewGuid(), "T-1", Guid.NewGuid(), 1, "Store A",
                    "Ana Ruiz", "CC 123", Base, 4.65d, -74.05d, 2),
                new ReportTripPodVm(Guid.NewGuid(), Guid.NewGuid(), "T-2", Guid.NewGuid(), 2, "Store B",
                    "Beto Paz", null, Base.AddHours(3), null, null, 0)));

        var result = await new TripPodExportReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Cell(0, "TripCode"), Is.EqualTo("T-2"));
            Assert.That(result.Cell(0, "ReceiverDocument"), Is.EqualTo(string.Empty));
            Assert.That(result.Cell(0, "Latitude"), Is.Null);
            Assert.That(result.Cell(0, "DocumentCount"), Is.EqualTo(0));
            Assert.That(result.Cell(1, "ReceiverName"), Is.EqualTo("Ana Ruiz"));
            Assert.That(result.Cell(1, "Latitude"), Is.EqualTo(4.65d));
            Assert.That(result.Cell(1, "Longitude"), Is.EqualTo(-74.05d));
            Assert.That(result.Cell(1, "DocumentCount"), Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Pod_ColumnOrderMatchesRowVmDeclaration()
    {
        _reader.Setup(r => r.GetTripProofsOfDeliveryAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(List<ReportTripPodVm>());

        var result = await new TripPodExportReport(_reader.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Columns.Select(c => c.PropertyName), Is.EqualTo(new[]
        {
            "TripCode", "StopSequence", "StopName", "ReceiverName", "ReceiverDocument",
            "CapturedAt", "Latitude", "Longitude", "DocumentCount"
        }));
    }

    // ---- feature gate ----

    [Test]
    public void AllReports_EnforceTheTripManagementFeatureBeforeReading()
    {
        _reader.Setup(r => r.EnsureTripManagementFeatureAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        IReport[] reports =
        [
            new TripSummaryReport(_reader.Object),
            new TripDetailReport(_reader.Object),
            new TripOnTimePerformanceReport(_reader.Object),
            new TripStopDwellReport(_reader.Object),
            new TripTollCostReport(_reader.Object),
            new TripPodExportReport(_reader.Object)
        ];

        foreach (var report in reports)
        {
            Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => report.GetDatasetAsync(_filters, CancellationToken.None), report.ReportCode);
        }

        _reader.Verify(r => r.GetTripsAsync(
            It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        _reader.Verify(r => r.GetTripProofsOfDeliveryAsync(
            It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---- resx column headers (all three Resources*.resx) ----

    private static readonly Type[] TripRowVms =
    [
        typeof(TripSummaryRowVm), typeof(TripStopDetailRowVm), typeof(TripOnTimePerformanceRowVm),
        typeof(TripStopDwellRowVm), typeof(TripTollCostRowVm), typeof(TripPodExportRowVm)
    ];

    // A resx key that is absent resolves to the raw key, so presence is asserted directly against the
    // resource set rather than inferred from the resolved text. The generated Resources class is
    // internal to the Domain assembly, so bind the ResourceManager by base name against that assembly.
    private static readonly ResourceManager Resources =
        new("TrackHub.Reporting.Domain.Resources.Resources", typeof(ReportHeaderResolver).Assembly);

    private static IEnumerable<string> ColumnKeys(Type rowVm)
        => rowVm.GetProperties().Select(p => p.Name);

    [TestCase("en")]
    [TestCase("es")]
    public void EveryTripColumnHasALocalizedHeader(string language)
    {
        var culture = new CultureInfo(language);
        // tryParents:false — the culture's own resx must carry the key; neutral fallback would hide
        // exactly the per-file inconsistency this test exists to catch.
        var set = Resources.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
        Assert.That(set, Is.Not.Null, $"No resource set for '{language}'.");

        foreach (var name in TripRowVms.SelectMany(ColumnKeys).Distinct())
        {
            var localized = set!.GetString(name);
            Assert.That(localized, Is.Not.Null, $"Missing '{language}' resx entry for column '{name}'.");
            Assert.That(localized, Is.Not.Empty, $"Empty '{language}' resx value for column '{name}'.");
            Assert.That(ReportHeaderResolver.Resolve(name, culture), Is.EqualTo(localized));
        }
    }

    // The neutral resx is what an unmatched culture falls back to; a key present only in .en/.es
    // would leak the raw property name for every other locale.
    [Test]
    public void EveryTripColumnHasANeutralHeader()
    {
        var set = Resources.GetResourceSet(CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: false);
        Assert.That(set, Is.Not.Null);

        foreach (var name in TripRowVms.SelectMany(ColumnKeys).Distinct())
        {
            Assert.That(set!.GetString(name), Is.Not.Null.And.Not.Empty, $"Missing neutral resx entry for column '{name}'.");
        }
    }

    // A header still equal to the raw PascalCase property name is an untranslated column leaking into
    // the export. These three happen to be correct English words, so they may equal their key in
    // `en` only — never in `es`.
    private static readonly string[] EnglishWordsIdenticalToTheirKey = ["Latitude", "Longitude", "Direction"];

    [TestCase("en")]
    [TestCase("es")]
    public void NoTripColumnHeaderIsTheRawPropertyName(string language)
    {
        var culture = new CultureInfo(language);

        foreach (var name in TripRowVms.SelectMany(ColumnKeys).Distinct())
        {
            if (language == "en" && EnglishWordsIdenticalToTheirKey.Contains(name))
            {
                continue;
            }

            Assert.That(ReportHeaderResolver.Resolve(name, culture), Is.Not.EqualTo(name),
                $"Column '{name}' resolves to its own key in '{language}'.");
        }
    }

    // ClosedXML table field names must be unique — two columns of the SAME report resolving to one
    // header both breaks the export and is indistinguishable to whoever reads it.
    [TestCase("en")]
    [TestCase("es")]
    public void NoTwoColumnsOfAReportShareAHeader(string language)
    {
        var culture = new CultureInfo(language);

        foreach (var rowVm in TripRowVms)
        {
            var byHeader = ColumnKeys(rowVm)
                .GroupBy(name => ReportHeaderResolver.Resolve(name, culture), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => $"'{g.Key}' ← {string.Join(", ", g)}")
                .ToList();

            Assert.That(byHeader, Is.Empty,
                $"{rowVm.Name} has duplicate '{language}' headers: {string.Join(" | ", byHeader)}");
        }
    }

    // One header wholly containing another in the same report (the spec 09 "Type"/"Qualification Type"
    // defect) passes an equality check but still makes a column meaningless to whoever reads the export.
    [TestCase("en")]
    [TestCase("es")]
    public void NoColumnHeaderIsContainedInAnotherHeaderOfTheSameReport(string language)
    {
        var culture = new CultureInfo(language);

        foreach (var rowVm in TripRowVms)
        {
            var headers = ColumnKeys(rowVm)
                .Select(name => (Key: name, Header: ReportHeaderResolver.Resolve(name, culture)))
                .ToList();

            foreach (var (key, header) in headers)
            {
                var swallowedBy = headers
                    .Where(other => other.Key != key
                        && other.Header.Contains(header, StringComparison.CurrentCultureIgnoreCase))
                    .Select(other => $"{other.Key} (\"{other.Header}\")")
                    .ToList();

                Assert.That(swallowedBy, Is.Empty,
                    $"{rowVm.Name}: '{language}' header \"{header}\" for column '{key}' is indistinguishable from {string.Join(", ", swallowedBy)}.");
            }
        }
    }
}
