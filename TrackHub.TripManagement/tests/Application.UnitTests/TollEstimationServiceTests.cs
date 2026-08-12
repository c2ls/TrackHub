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

using TrackHub.TripManagement.Application.Tolls.Services;

namespace TrackHub.TripManagement.Application.UnitTests;

/// <summary>
/// Acceptance 21 — the status trichotomy. The <c>PartialNoTariff</c> case is the important one:
/// an estimate that quietly understates cost is worse than no estimate.
/// </summary>
[TestFixture]
public class TollEstimationServiceTests
{
    private static readonly IReadOnlyCollection<CoordinateVm> Route = [new(4.65, -74.05), new(4.75, -74.15)];
    private static readonly DateOnly OnDate = new(2026, 7, 21);

    [Test]
    public async Task EmptyCatalog_YieldsNoStationsAndANullAmount()
    {
        var service = Service([]);

        var estimate = await service.EstimateAsync(Route, "II", OnDate, 100d, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(estimate.TollStatus, Is.EqualTo(TollStatuses.NoStations));
            Assert.That(estimate.EstimatedTollAmount, Is.Null, "zero would read as 'this route is free', which is a different claim");
            Assert.That(estimate.Stations, Is.Empty);
        });
    }

    [Test]
    public async Task EveryMatchPriced_YieldsComputedAndTheSum()
    {
        var service = Service([TestFactory.Match(true, 12500m), TestFactory.Match(true, 7300m)]);

        var estimate = await service.EstimateAsync(Route, "II", OnDate, 100d, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(estimate.TollStatus, Is.EqualTo(TollStatuses.Computed));
            Assert.That(estimate.EstimatedTollAmount, Is.EqualTo(19800m));
            Assert.That(estimate.Currency, Is.EqualTo("COP"));
        });
    }

    [Test]
    public async Task OneUnpricedMatch_YieldsPartialNoTariffWithThePricedSubtotal()
    {
        var service = Service([TestFactory.Match(true, 12500m), TestFactory.Match(false, null)]);

        var estimate = await service.EstimateAsync(Route, "II", OnDate, 100d, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(estimate.TollStatus, Is.EqualTo(TollStatuses.PartialNoTariff));
            Assert.That(estimate.EstimatedTollAmount, Is.EqualTo(12500m), "the priced subtotal, with the gap declared rather than netted to zero");
            Assert.That(estimate.Stations, Has.Count.EqualTo(2), "the unpriced station is still reported so the gap is visible");
        });
    }

    [Test]
    public async Task NoMatchPriced_YieldsPartialNoTariffWithANullAmount()
    {
        var service = Service([TestFactory.Match(false, null), TestFactory.Match(false, null)]);

        var estimate = await service.EstimateAsync(Route, "II", OnDate, 100d, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(estimate.TollStatus, Is.EqualTo(TollStatuses.PartialNoTariff));
            Assert.That(estimate.EstimatedTollAmount, Is.Null, "a zero here would present an understated cost as a fact");
        });
    }

    /// <summary>
    /// <b>Documents current behaviour, which I believe is a genuine defect.</b>
    /// <para>
    /// The service sums <c>Amount</c> across every priced match and labels the total with
    /// <c>currency ??= match.Currency</c> — the FIRST currency it saw. On a cross-border route
    /// (Cucuta–San Antonio, Ipiales–Tulcan: both ordinary Colombian freight corridors) the matches
    /// carry COP and VES/USD, and the operator is shown one number, in one currency, that is the
    /// arithmetic sum of two. 12 500 COP + 3.50 USD is reported as "12 503.50 COP".
    /// </para>
    /// <para>
    /// This is the same class of lie the <c>PartialNoTariff</c> status exists to prevent — the
    /// service already refuses to net an unpriced station to zero because an estimate that
    /// misstates cost is worse than no estimate. A mixed-currency sum misstates it far more
    /// severely, and silently. The honest answer is a third status (or a per-currency breakdown),
    /// but that is a contract change affecting the GraphQL schema and the portal, so it is REPORTED
    /// here rather than changed under cover of a test.
    /// </para>
    /// </summary>
    [Test]
    public async Task MixedCurrencies_RefuseATotalInsteadOfSummingAcrossThem()
    {
        var service = Service([
            TestFactory.Match(true, 12_500m, "COP"),
            TestFactory.Match(true, 3.50m, "USD")]);

        var estimate = await service.EstimateAsync(Route, "II", OnDate, 100d, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(estimate.TollStatus, Is.EqualTo(TollStatuses.MixedCurrency));
            Assert.That(estimate.EstimatedTollAmount, Is.Null,
                "12 500 COP + 3.50 USD has no correct total — reporting one is worse than reporting none");
            Assert.That(estimate.Currency, Is.Null, "no single currency labels this route");

            // The breakdown survives: the operator still sees each station and what it costs in its
            // own currency, which is the honest form of the answer.
            Assert.That(estimate.Stations, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task MixedCurrencies_GiveTheSameAnswerRegardlessOfMatchOrder()
    {
        // What made the old behaviour dangerous: the very same route priced in the other order was
        // reported in USD. Nothing about the number changed, only its label — so the answer depended
        // on the order PostGIS happened to return matches in.
        var service = Service([
            TestFactory.Match(true, 3.50m, "USD"),
            TestFactory.Match(true, 12_500m, "COP")]);

        var estimate = await service.EstimateAsync(Route, "II", OnDate, 100d, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(estimate.TollStatus, Is.EqualTo(TollStatuses.MixedCurrency));
            Assert.That(estimate.Currency, Is.Null);
        });
    }

    [Test]
    public async Task ASingleCurrencyRoute_IsLabelledWithThatCurrency()
    {
        // The normal, correct case — kept alongside the two above so a future fix for mixed
        // currencies is not free to break the ordinary domestic route.
        var service = Service([
            TestFactory.Match(true, 12_500m, "COP"),
            TestFactory.Match(true, 7_300m, "COP")]);

        var estimate = await service.EstimateAsync(Route, "II", OnDate, 100d, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(estimate.EstimatedTollAmount, Is.EqualTo(19_800m));
            Assert.That(estimate.Currency, Is.EqualTo("COP"));
        });
    }

    [Test]
    public async Task AnUnpricedMatchDoesNotSupplyTheCurrencyLabel()
    {
        // A station with no tariff for this class carries a null Currency; letting it win the
        // `??=` race would label a real total with nothing at all.
        var service = Service([
            TestFactory.Match(false, null),
            TestFactory.Match(true, 7_300m, "COP")]);

        var estimate = await service.EstimateAsync(Route, "II", OnDate, 100d, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(estimate.TollStatus, Is.EqualTo(TollStatuses.PartialNoTariff));
            Assert.That(estimate.Currency, Is.EqualTo("COP"));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    public async Task WithoutAVehicleClass_YieldsNotComputed(string? vehicleClass)
    {
        var reader = new Mock<ITollCatalogReader>(MockBehavior.Strict);
        var service = new TollEstimationService(reader.Object);

        var estimate = await service.EstimateAsync(Route, vehicleClass, OnDate, 100d, CancellationToken.None);

        Assert.That(estimate.TollStatus, Is.EqualTo(TollStatuses.NotComputed));
        reader.VerifyNoOtherCalls();
    }

    [Test]
    public async Task AnEmptyRoute_YieldsNotComputedWithoutQueryingTheCatalog()
    {
        var reader = new Mock<ITollCatalogReader>(MockBehavior.Strict);
        var service = new TollEstimationService(reader.Object);

        var estimate = await service.EstimateAsync([], "II", OnDate, 100d, CancellationToken.None);

        Assert.That(estimate.TollStatus, Is.EqualTo(TollStatuses.NotComputed));
        reader.VerifyNoOtherCalls();
    }

    private static TollEstimationService Service(IReadOnlyCollection<TollStationMatchVm> matches)
    {
        var reader = new Mock<ITollCatalogReader>();
        reader.Setup(r => r.MatchStationsAsync(
                It.IsAny<IReadOnlyCollection<CoordinateVm>>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);
        return new TollEstimationService(reader.Object);
    }
}
