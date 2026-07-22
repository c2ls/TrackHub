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

using System.Text.Json;
using Common.Application.Interfaces;
using TrackHub.TripManagement.Application.Tolls.Services.Interfaces;
using TrackHub.TripManagement.Application.Trips.Commands.PlanRoute;
using TrackHub.TripManagement.Domain.Exceptions;

namespace TrackHub.TripManagement.Application.UnitTests;

/// <summary>
/// Acceptance 18: ORS unavailable or unconfigured records a <c>Failed</c> plan with an error code,
/// the trip stays fully usable, and <b>no exception reaches the caller</b>. A routing outage is an
/// operational fact about a third party, not a reason a dispatcher cannot run their fleet.
/// </summary>
[TestFixture]
public class PlanTripRouteCommandHandlerTests
{
    [Test]
    public async Task UnconfiguredProvider_RecordsAFailedPlanAndReturnsItWithoutThrowing()
    {
        var harness = new PlanHarness(configured: false);

        var plan = await harness.Handler().Handle(
            new PlanTripRouteCommand(TestFactory.TripId, null, null), CancellationToken.None);

        Assert.That(plan.Status, Is.EqualTo(RoutePlanStatuses.Failed));
        harness.RoutePlanWriter.Verify(w => w.SaveFailedPlanAsync(
            TestFactory.TripId, TestFactory.AccountId, It.IsAny<string>(), 500,
            TripErrorCodes.RoutingNotConfigured, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        harness.RoutePlanWriter.Verify(w => w.SaveReadyPlanAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<CoordinateVm>>(), It.IsAny<int>(),
            It.IsAny<double>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<TollEstimateVm>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ProviderOutage_RecordsAFailedPlanCarryingTheProviderErrorCode()
    {
        var harness = new PlanHarness();
        harness.RoutingProvider
            .Setup(p => p.GetRouteAsync(It.IsAny<IReadOnlyCollection<CoordinateVm>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RoutingUnavailableException(TripErrorCodes.RoutingUnavailable, "502 from ORS"));

        var plan = await harness.Handler().Handle(
            new PlanTripRouteCommand(TestFactory.TripId, null, null), CancellationToken.None);

        Assert.That(plan.Status, Is.EqualTo(RoutePlanStatuses.Failed));
        harness.RoutePlanWriter.Verify(w => w.SaveFailedPlanAsync(
            TestFactory.TripId, TestFactory.AccountId, It.IsAny<string>(), It.IsAny<int>(),
            TripErrorCodes.RoutingUnavailable, "502 from ORS", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SuccessfulPlan_ComputesTheTollEstimateAndStoresAReadyPlan()
    {
        var harness = new PlanHarness();

        var plan = await harness.Handler().Handle(
            new PlanTripRouteCommand(TestFactory.TripId, 800, "III"), CancellationToken.None);

        Assert.That(plan.Status, Is.EqualTo(RoutePlanStatuses.Ready));
        harness.TollEstimationService.Verify(s => s.EstimateAsync(
            It.IsAny<IReadOnlyCollection<CoordinateVm>>(), "III", It.IsAny<DateOnly>(), It.IsAny<double>(), It.IsAny<CancellationToken>()), Times.Once);
        harness.RoutePlanWriter.Verify(w => w.SaveReadyPlanAsync(
            TestFactory.TripId, TestFactory.AccountId, It.IsAny<string>(), It.IsAny<IReadOnlyCollection<CoordinateVm>>(), 800,
            1000d, 600, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<TollEstimateVm>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // Acceptance 17 requires route planning to return a PER-LEG BREAKDOWN. The provider parses the
    // legs; the handler used to pass legsJson: null / waypointsJson: null, so §6.1's columns were
    // permanently empty and no leg data ever reached a caller.
    [Test]
    public async Task SuccessfulPlan_PersistsTheLegBreakdownAndTheOrderedWaypoints()
    {
        var harness = new PlanHarness();

        await harness.Handler().Handle(new PlanTripRouteCommand(TestFactory.TripId, null, null), CancellationToken.None);

        Assert.That(harness.LegsJson, Is.Not.Null);
        Assert.That(harness.WaypointsJson, Is.Not.Null);

        // Round-trips back through the same Web-defaults contract the column is written with.
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var legs = JsonSerializer.Deserialize<RouteLegVm[]>(harness.LegsJson!, options);
        var waypoints = JsonSerializer.Deserialize<CoordinateVm[]>(harness.WaypointsJson!, options);

        Assert.That(legs, Is.Not.Null);
        Assert.That(legs!, Has.Length.EqualTo(2));
        Assert.That(legs![0].DistanceMeters, Is.EqualTo(400d));
        Assert.That(legs![1].DurationSeconds, Is.EqualTo(360));

        // Origin first, then the stops in sequence — the order a caller has to be able to rely on.
        Assert.That(waypoints, Is.Not.Null);
        Assert.That(waypoints!, Has.Length.EqualTo(2));
        Assert.That(waypoints![0].Latitude, Is.EqualTo(4.65));
        Assert.That(waypoints![1].Latitude, Is.EqualTo(4.7));

        // camelCase, matching the AlertEmitter payload precedent.
        Assert.That(harness.LegsJson, Does.Contain("distanceMeters"));
        Assert.That(harness.WaypointsJson, Does.Contain("latitude"));
    }

    // Acceptance 18 says a routing call leaves the trip fully usable with no exception reaching the
    // caller. Toll estimation runs ST_DWithin against the catalog, so a PostGIS or catalog failure
    // used to surface as an unhandled 500 from a ROUTE PLANNING call.
    [Test]
    public async Task TollEstimationFailure_StillSavesTheRouteWithoutAnEstimate()
    {
        var harness = new PlanHarness();
        harness.TollEstimationService
            .Setup(s => s.EstimateAsync(
                It.IsAny<IReadOnlyCollection<CoordinateVm>>(), It.IsAny<string?>(), It.IsAny<DateOnly>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("ST_DWithin: PostGIS is unavailable"));

        var plan = await harness.Handler().Handle(
            new PlanTripRouteCommand(TestFactory.TripId, null, "III"), CancellationToken.None);

        Assert.That(plan.Status, Is.EqualTo(RoutePlanStatuses.Ready));
        Assert.That(harness.SavedEstimate.TollStatus, Is.EqualTo(TollStatuses.NotComputed));
        Assert.That(harness.SavedEstimate.EstimatedTollAmount, Is.Null);
        harness.RoutePlanWriter.Verify(w => w.SaveFailedPlanAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task AccountConfigFailure_AlsoDegradesRatherThanFailingThePlanningCall()
    {
        var harness = new PlanHarness();
        harness.AccountFeatureReader
            .Setup(r => r.GetAccountConfigAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("account config read failed"));

        var plan = await harness.Handler().Handle(
            new PlanTripRouteCommand(TestFactory.TripId, null, null), CancellationToken.None);

        Assert.That(plan.Status, Is.EqualTo(RoutePlanStatuses.Ready));
        Assert.That(harness.SavedEstimate.TollStatus, Is.EqualTo(TollStatuses.NotComputed));
    }

    [Test]
    public async Task CorridorWidth_IsClampedToTheSupportedRange()
    {
        var harness = new PlanHarness();

        await harness.Handler().Handle(new PlanTripRouteCommand(TestFactory.TripId, 99999, null), CancellationToken.None);

        harness.RoutePlanWriter.Verify(w => w.SaveReadyPlanAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<CoordinateVm>>(), 5000,
            It.IsAny<double>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<TollEstimateVm>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class PlanHarness
    {
        public PlanHarness(bool configured = true)
        {
            RoutingProvider.SetupGet(p => p.Name).Returns(RoutePlanProviders.OpenRouteService);
            RoutingProvider.SetupGet(p => p.IsConfigured).Returns(configured);
            RoutingProvider
                .Setup(p => p.GetRouteAsync(It.IsAny<IReadOnlyCollection<CoordinateVm>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RouteResultVm(
                    [new CoordinateVm(4.65, -74.05), new CoordinateVm(4.7, -74.0)],
                    1000d,
                    600,
                    [new RouteLegVm(0, 400d, 240), new RouteLegVm(1, 600d, 360)]));

            Reader.Setup(r => r.GetTripDetailAsync(TestFactory.TripId, TestFactory.AccountId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TripDetailVm(TestFactory.Trip(), [TestFactory.Stop()], null, null, [], []));

            RoutePlanWriter
                .Setup(w => w.SaveReadyPlanAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<CoordinateVm>>(), It.IsAny<int>(),
                    It.IsAny<double>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<TollEstimateVm>(), It.IsAny<CancellationToken>()))
                .Callback((Guid _, Guid _, string _, IReadOnlyCollection<CoordinateVm> _, int _, double _, int _,
                    string? waypointsJson, string? legsJson, TollEstimateVm estimate, CancellationToken _) =>
                {
                    WaypointsJson = waypointsJson;
                    LegsJson = legsJson;
                    SavedEstimate = estimate;
                })
                .ReturnsAsync(Plan(RoutePlanStatuses.Ready));
            RoutePlanWriter
                .Setup(w => w.SaveFailedPlanAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Plan(RoutePlanStatuses.Failed));

            TollEstimationService
                .Setup(s => s.EstimateAsync(
                    It.IsAny<IReadOnlyCollection<CoordinateVm>>(), It.IsAny<string?>(), It.IsAny<DateOnly>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TollEstimateVm("II", null, null, TollStatuses.NoStations, []));

            AccountFeatureReader
                .Setup(r => r.GetAccountConfigAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TripAccountConfigVm.Default);

            EventWriter
                .Setup(w => w.AppendAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        public string? WaypointsJson { get; private set; }

        public string? LegsJson { get; private set; }

        public TollEstimateVm SavedEstimate { get; private set; }

        public Mock<ITripReader> Reader { get; } = new();

        public Mock<IRoutePlanWriter> RoutePlanWriter { get; } = new();

        public Mock<IRoutingProvider> RoutingProvider { get; } = new();

        public Mock<ITollEstimationService> TollEstimationService { get; } = new();

        public Mock<ITripEventWriter> EventWriter { get; } = new();

        public Mock<IAccountFeatureReader> AccountFeatureReader { get; } = new();

        public Mock<IUser> User { get; } = TestFactory.User();

        public Mock<IUserReader> UserReader { get; } = TestFactory.UserReader();

        public PlanTripRouteCommandHandler Handler()
            => new(Reader.Object, RoutePlanWriter.Object, RoutingProvider.Object, TollEstimationService.Object,
                EventWriter.Object, AccountFeatureReader.Object, UserReader.Object, User.Object,
                TestFactory.Logger<PlanTripRouteCommandHandler>());

        private static RoutePlanVm Plan(string status)
            => new(TestFactory.RoutePlanId, TestFactory.AccountId, TestFactory.TripId, RoutePlanProviders.OpenRouteService,
                null, null, 500, 0d, 0, null, null, DateTimeOffset.UtcNow, status, null, null, null, null, null,
                TollStatuses.NotComputed, []);
    }
}
