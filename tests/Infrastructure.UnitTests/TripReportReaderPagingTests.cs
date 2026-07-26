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

using System.Text;
using System.Text.Json;
using Common.Application.Interfaces;
using GraphQL;
using GraphQL.Client.Abstractions;
using Moq;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Infrastructure.GraphQLApi;

namespace TrackHub.Reporting.Infrastructure.UnitTests;

// `TripReportReader.DrainAsync` is the single paging loop behind all four trip feeds, and it is what
// stands between a 501-trip account and a silently truncated export. It is private, so it is
// exercised through the public reads; the seam is `IGraphQLClient.SendQueryAsync`, which lets a fake
// server return `{ items, totalCount }` payloads without a live TripManagement.
[TestFixture]
public class TripReportReaderPagingTests
{
    private const int PageSize = 500;   // Mirrors TripReportReader.PageSize (producer's take clamp).
    private const int MaxRows = 100_000; // Mirrors TripReportReader.MaxRows (defensive source cap).

    private readonly Guid _accountId = Guid.NewGuid();
    private Mock<IGraphQLClient> _client = null!;
    private TripReportReader _reader = null!;

    // Every (skip, take) pair the reader asked for, in order.
    private List<(int Skip, int Take)> _requests = null!;

    [SetUp]
    public void SetUp()
    {
        _requests = [];
        _client = new Mock<IGraphQLClient>();

        var factory = new Mock<IGraphQLClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(_client.Object);

        var user = new Mock<IUser>();
        user.SetupGet(u => u.AccountId).Returns(_accountId);

        var features = new Mock<IAccountFeatureReader>();
        features
            .Setup(f => f.EnsureFeatureEnabledAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _reader = new TripReportReader(factory.Object, user.Object, features.Object);
    }

    // The reader builds `Variables` as an anonymous object; read skip/take back off it reflectively
    // so the assertions do not depend on the query text.
    private static (int Skip, int Take) ReadWindow(GraphQLRequest request)
    {
        var variables = request.Variables!;
        var type = variables.GetType();
        return ((int)type.GetProperty("skip")!.GetValue(variables)!,
                (int)type.GetProperty("take")!.GetValue(variables)!);
    }

    // A `tripReportData` payload of `count` rows and a declared `totalCount`, shaped exactly like
    // TripManagement's camelCase response. GraphQLService reads `response.Data.ToString()`, so a
    // JsonElement is the faithful stand-in.
    private static JsonElement TripPage(int count, int totalCount)
        => TripPage(count, totalCount, count, count < totalCount);

    private static JsonElement TripPage(int count, int totalCount, int nextSkip, bool hasMore)
    {
        var json = new StringBuilder("{\"tripReportData\":{\"items\":[");
        for (var i = 0; i < count; i++)
        {
            if (i > 0)
            {
                json.Append(',');
            }

            json.Append($$"""
                {"tripId":"{{Guid.NewGuid()}}","code":"T-{{i}}","status":"Completed",
                 "transporterId":"{{Guid.NewGuid()}}","transporterName":"Truck","driverId":null,"driverName":null,
                 "customerName":null,"plannedStartAt":"2026-07-01T00:00:00+00:00","plannedEndAt":null,
                 "actualStartAt":null,"actualEndAt":null,"plannedDistanceMeters":null,"actualDistanceMeters":0,
                 "stopCount":0,"estimatedTollAmount":null,"tollCurrency":null,"tollStatus":"NoStations"}
                """);
        }

        json.Append($"],\"totalCount\":{totalCount},\"nextSkip\":{nextSkip},\"hasMore\":{(hasMore ? "true" : "false")}}}}}");
        return JsonDocument.Parse(json.ToString()).RootElement.Clone();
    }

    /// <summary>
    /// Stands in for TripManagement: records the requested window and serves <paramref name="pageSizes"/>
    /// in order — one entry per expected round trip, all declaring <paramref name="totalCount"/>.
    /// Requests beyond the script fail the test, which is how a runaway loop is caught rather than
    /// hanging the run.
    /// </summary>
    private void RespondWith(int totalCount, params int[] pageSizes)
    {
        var consumed = 0;

        _client
            .Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .Returns((GraphQLRequest request, CancellationToken _) =>
            {
                _requests.Add(ReadWindow(request));

                Assert.That(_requests, Has.Count.LessThanOrEqualTo(pageSizes.Length),
                    $"DrainAsync issued request #{_requests.Count} but only {pageSizes.Length} pages were scripted — the loop is not terminating.");

                var count = pageSizes[_requests.Count - 1];
                consumed += count;

                // This feed's producer unit IS its row unit, so the cursor is just the running
                // total. The toll feed's is not — see TollFeedExpandingRowsPerPage_… below.
                return Task.FromResult(new GraphQLResponse<object>
                {
                    Data = TripPage(count, totalCount, consumed, consumed < totalCount)
                });
            });
    }

    [Test]
    public async Task SinglePartialPage_StopsAfterOneRequest()
    {
        RespondWith(37, 37);

        var trips = await _reader.GetTripsAsync(null, null, null, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(trips, Has.Count.EqualTo(37));
            Assert.That(_requests, Is.EqualTo(new[] { (0, PageSize) }));
        });
    }

    // Skip advances by the producer's cursor. A reader that reset skip (or advanced by a fixed page
    // index) would re-fetch or skip a page and nobody would notice.
    [Test]
    public async Task MultiplePages_AdvanceSkipByTheProducerCursor()
    {
        RespondWith((PageSize * 2) + 12, PageSize, PageSize, 12);

        var trips = await _reader.GetTripsAsync(null, null, null, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(trips, Has.Count.EqualTo((PageSize * 2) + 12));
            Assert.That(_requests, Is.EqualTo(new[] { (0, PageSize), (PageSize, PageSize), (PageSize * 2, PageSize) }));
        });
    }

    // An exact multiple of the page size still terminates without a probe page, because HasMore —
    // not the page length — is the authoritative end condition.
    [Test]
    public async Task ExactMultipleOfPageSize_TerminatesOnHasMoreWithoutAProbePage()
    {
        RespondWith(PageSize * 2, PageSize, PageSize);

        var trips = await _reader.GetTripsAsync(null, null, null, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(trips, Has.Count.EqualTo(PageSize * 2));
            Assert.That(_requests, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task EmptyFirstPage_ReturnsNothingAndDoesNotPageAgain()
    {
        RespondWith(0, 0);

        var trips = await _reader.GetTripsAsync(null, null, null, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(trips, Is.Empty);
            Assert.That(_requests, Has.Count.EqualTo(1));
        });
    }

    // A producer that keeps returning full pages with an absurd totalCount must not be followed
    // forever: the loop is bounded by the 100k defensive source-fetch cap.
    [Test]
    public async Task AlwaysFullPages_StopsAtTheDefensiveRowCapInsteadOfLoopingForever()
    {
        var requests = 0;
        _client
            .Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .Returns((GraphQLRequest request, CancellationToken _) =>
            {
                requests++;
                Assert.That(requests, Is.LessThanOrEqualTo(250), "DrainAsync is not bounded by the row cap.");
                return Task.FromResult(new GraphQLResponse<object>
                {
                    Data = TripPage(PageSize, int.MaxValue, requests * PageSize, true)
                });
            });

        var trips = await _reader.GetTripsAsync(null, null, null, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(requests, Is.EqualTo(MaxRows / PageSize));
            Assert.That(trips, Has.Count.EqualTo(MaxRows));
        });
    }

    /// <summary>
    /// The regression this whole cursor contract exists for.
    /// <para>
    /// The toll feed pages ROUTE PLANS and expands each into one row per matched station, so its row
    /// count and its paging unit differ. The old loop skipped by rows collected and stopped once
    /// that count reached <c>totalCount</c> — with 700 plans averaging three stations, page one
    /// returned 1500 rows against a totalCount of 700, so it stopped after ONE request and dropped
    /// 200 trips from a financial report with no error raised anywhere.
    /// </para>
    /// <para>
    /// Scripted here at 1:3 expansion: two pages of 500 plans then a final 200, all with a plan-unit
    /// cursor. A loop that reverts to counting rows fetches one page and returns 1500 instead of
    /// 2100, and this test fails.
    /// </para>
    /// </summary>
    [Test]
    public async Task TollFeedExpandingRowsPerPage_PagesByPlansAndReturnsEveryRow()
    {
        const int totalPlans = (PageSize * 2) + 200;
        const int stationsPerPlan = 3;
        var planPages = new[] { PageSize, PageSize, 200 };
        var consumedPlans = 0;

        _client
            .Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .Returns((GraphQLRequest request, CancellationToken _) =>
            {
                _requests.Add(ReadWindow(request));

                Assert.That(_requests, Has.Count.LessThanOrEqualTo(planPages.Length),
                    "DrainAsync asked for more pages than the producer has plans — it is not following the cursor.");

                var plans = planPages[_requests.Count - 1];
                consumedPlans += plans;

                var json = "{\"tripTollReportData\":{\"items\":["
                    + TollRows(plans * stationsPerPlan)
                    + $"],\"totalCount\":{totalPlans},\"nextSkip\":{consumedPlans},"
                    + $"\"hasMore\":{(consumedPlans < totalPlans ? "true" : "false")}}}}}";

                return Task.FromResult(new GraphQLResponse<object>
                {
                    Data = JsonDocument.Parse(json).RootElement.Clone()
                });
            });

        var tolls = await _reader.GetTripTollsAsync(null, null, null, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(tolls, Has.Count.EqualTo(totalPlans * stationsPerPlan),
                "Station rows were dropped — the drain stopped before the last plan.");

            // Skip advances in PLANS (0, 500, 1000), never in the 1500-row page length.
            Assert.That(_requests, Is.EqualTo(new[] { (0, PageSize), (PageSize, PageSize), (PageSize * 2, PageSize) }));
        });
    }

    /// <summary>
    /// A producer that claims <c>HasMore</c> without advancing its cursor is a producer-side bug.
    /// The loop must break rather than spin, or a report request hangs instead of failing.
    /// </summary>
    [Test]
    public async Task ProducerReportsHasMoreButDoesNotAdvance_BreaksInsteadOfLoopingForever()
    {
        var requests = 0;
        _client
            .Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .Returns((GraphQLRequest request, CancellationToken _) =>
            {
                requests++;
                Assert.That(requests, Is.LessThanOrEqualTo(5), "DrainAsync is spinning on a stuck cursor.");
                return Task.FromResult(new GraphQLResponse<object>
                {
                    Data = TripPage(10, 999, nextSkip: 0, hasMore: true)
                });
            });

        var trips = await _reader.GetTripsAsync(null, null, null, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(requests, Is.EqualTo(1));
            Assert.That(trips, Has.Count.EqualTo(10));
        });
    }

    // The account id is never taken from the caller's filters — it comes from the propagated token,
    // which is what keeps one tenant's report from ever addressing another's data.
    [Test]
    public async Task EveryRequestCarriesTheTokenAccountIdAndTheFilterArguments()
    {
        var from = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 7, 31, 0, 0, 0, TimeSpan.Zero);
        var transporterId = Guid.NewGuid();
        var driverId = Guid.NewGuid();

        GraphQLRequest? captured = null;
        _client
            .Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .Returns((GraphQLRequest request, CancellationToken _) =>
            {
                captured = request;
                return Task.FromResult(new GraphQLResponse<object> { Data = TripPage(0, 0) });
            });

        await _reader.GetTripsAsync(from, to, transporterId, driverId, CancellationToken.None);

        var variables = captured!.Variables!;
        var type = variables.GetType();
        Assert.Multiple(() =>
        {
            Assert.That(type.GetProperty("accountId")!.GetValue(variables), Is.EqualTo(_accountId));
            Assert.That(type.GetProperty("from")!.GetValue(variables), Is.EqualTo(from));
            Assert.That(type.GetProperty("to")!.GetValue(variables), Is.EqualTo(to));
            Assert.That(type.GetProperty("transporterId")!.GetValue(variables), Is.EqualTo(transporterId));
            Assert.That(type.GetProperty("driverId")!.GetValue(variables), Is.EqualTo(driverId));
        });
    }

    // The stop, toll and POD feeds are separate query documents but share one drain loop; prove each
    // entry point pages rather than one-shot fetching, and that each reads its own root field.
    [Test]
    public async Task StopTollAndPodFeeds_DrainThroughTheSameLoop()
    {
        var pages = new Queue<string>(
        [
            $"{{\"tripStopReportData\":{{\"items\":[{StopRows(PageSize)}],\"totalCount\":{PageSize + 1},\"nextSkip\":{PageSize},\"hasMore\":true}}}}",
            "{\"tripStopReportData\":{\"items\":[],\"totalCount\":501,\"nextSkip\":501,\"hasMore\":false}}",
            "{\"tripTollReportData\":{\"items\":[" + TollRows(2) + "],\"totalCount\":2,\"nextSkip\":2,\"hasMore\":false}}",
            "{\"tripPodReportData\":{\"items\":[" + PodRows(3) + "],\"totalCount\":3,\"nextSkip\":3,\"hasMore\":false}}"
        ]);

        _client
            .Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .Returns((GraphQLRequest request, CancellationToken _) =>
            {
                _requests.Add(ReadWindow(request));
                return Task.FromResult(new GraphQLResponse<object>
                {
                    Data = JsonDocument.Parse(pages.Dequeue()).RootElement.Clone()
                });
            });

        var stops = await _reader.GetTripStopsAsync(null, null, null, null, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(stops, Has.Count.EqualTo(PageSize));
            Assert.That(_requests, Is.EqualTo(new[] { (0, PageSize), (PageSize, PageSize) }));
        });

        _requests.Clear();
        var tolls = await _reader.GetTripTollsAsync(null, null, null, null, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(tolls, Has.Count.EqualTo(2));
            Assert.That(tolls.First().HasTariff, Is.True);
            Assert.That(_requests, Is.EqualTo(new[] { (0, PageSize) }));
        });

        _requests.Clear();
        var pods = await _reader.GetTripProofsOfDeliveryAsync(null, null, null, null, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(pods, Has.Count.EqualTo(3));
            Assert.That(_requests, Is.EqualTo(new[] { (0, PageSize) }));
        });
    }

    private static string StopRows(int count)
        => string.Join(',', Enumerable.Range(0, count).Select(i => $$"""
            {"tripStopId":"{{Guid.NewGuid()}}","tripId":"{{Guid.NewGuid()}}","tripCode":"T-{{i}}",
             "transporterName":"Truck","driverName":null,"customerName":null,"sequence":{{i}},"name":"S{{i}}",
             "status":"Departed","plannedArrivalFrom":null,"plannedArrivalTo":null,"actualArrivalAt":null,
             "actualDepartureAt":null,"deliveryCount":0,"deliveredCount":0,"failedDeliveryCount":0,
             "partialDeliveryCount":0}
            """));

    private static string TollRows(int count)
        => string.Join(',', Enumerable.Range(0, count).Select(i => $$"""
            {"tripId":"{{Guid.NewGuid()}}","tripCode":"T-{{i}}","routePlanId":"{{Guid.NewGuid()}}",
             "plannedStartAt":"2026-07-01T00:00:00+00:00","tollVehicleClass":"C3",
             "tollStationId":"{{Guid.NewGuid()}}","stationName":"Peaje {{i}}","stationCode":"P{{i}}",
             "roadName":"Ruta 45","direction":"N","amount":12000.0,"currency":"COP","hasTariff":true}
            """));

    private static string PodRows(int count)
        => string.Join(',', Enumerable.Range(0, count).Select(i => $$"""
            {"proofOfDeliveryId":"{{Guid.NewGuid()}}","tripId":"{{Guid.NewGuid()}}","tripCode":"T-{{i}}",
             "tripStopId":"{{Guid.NewGuid()}}","stopSequence":{{i}},"stopName":"S{{i}}",
             "receiverName":"R{{i}}","receiverDocument":null,"capturedAt":"2026-07-01T00:00:00+00:00",
             "latitude":null,"longitude":null,"documentCount":0}
            """));
}
