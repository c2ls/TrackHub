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
using System.Text;
using System.Text.Json;
using Common.Application.Interfaces;
using GraphQL;
using GraphQL.Client.Abstractions;
using Moq;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Infrastructure.GraphQLApi;

namespace TrackHub.Reporting.Infrastructure.UnitTests;

// `WorkforceReportReader.FetchAllAsync` is the only paging loop in the workforce readers, and it is
// what stands between a 501-driver account and a silently truncated export. It is private, so it is
// exercised through the public reads; the seam is `IGraphQLClient.SendQueryAsync`, which lets a fake
// server return page-shaped payloads without a live Manager.
[TestFixture]
public class WorkforceReportReaderPagingTests
{
    private const int PageSize = 500; // Mirrors WorkforceReportReader.PageSize (Manager's take clamp).

    private readonly Guid _accountId = Guid.NewGuid();
    private Mock<IGraphQLClient> _client = null!;
    private WorkforceReportReader _reader = null!;

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

        _reader = new WorkforceReportReader(factory.Object, user.Object, features.Object);
    }

    /// <summary>
    /// Stands in for Manager: records the requested window and serves <paramref name="pageSizes"/>
    /// in order — one entry per expected round trip. Requests beyond the script fail the test, which
    /// is how a runaway loop is caught rather than hanging the run.
    /// </summary>
    private void RespondWith(params int[] pageSizes)
    {
        _client
            .Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .Returns((GraphQLRequest request, CancellationToken _) =>
            {
                var (skip, take) = ReadWindow(request);
                _requests.Add((skip, take));

                Assert.That(_requests, Has.Count.LessThanOrEqualTo(pageSizes.Length),
                    $"FetchAllAsync issued request #{_requests.Count} but only {pageSizes.Length} pages were scripted — the loop is not terminating.");

                return Task.FromResult(new GraphQLResponse<object>
                {
                    Data = DriverPage(pageSizes[_requests.Count - 1])
                });
            });
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

    // A `driversByAccount` payload of `count` rows, shaped exactly like Manager's camelCase response.
    // GraphQLService reads `response.Data.ToString()`, so a JsonElement is the faithful stand-in.
    private static JsonElement DriverPage(int count)
    {
        var json = new StringBuilder("{\"driversByAccount\":[");
        for (var i = 0; i < count; i++)
        {
            if (i > 0)
            {
                json.Append(',');
            }

            json.Append(CultureInfo.InvariantCulture, $$"""
                {"driverId":"{{Guid.NewGuid()}}","name":"Driver {{i}}","phone":null,"documentType":null,
                 "documentNumber":null,"active":true,"employeeCode":null,"licenseNumber":null,
                 "licenseExpiresAt":null,"defaultTransporterId":null}
                """);
        }

        json.Append("]}");
        return JsonDocument.Parse(json.ToString()).RootElement.Clone();
    }

    [Test]
    public async Task SinglePartialPage_StopsAfterOneRequest()
    {
        RespondWith(37);

        var drivers = await _reader.GetDriversAsync(CancellationToken.None);

        Assert.That(drivers, Has.Count.EqualTo(37));
        Assert.That(_requests, Is.EqualTo(new[] { (0, PageSize) }));
    }

    [Test]
    public async Task ShortFinalPage_AccumulatesEveryPageAndStops()
    {
        RespondWith(PageSize, PageSize, 12);

        var drivers = await _reader.GetDriversAsync(CancellationToken.None);

        Assert.That(drivers, Has.Count.EqualTo((PageSize * 2) + 12));
        Assert.That(_requests, Is.EqualTo(new[] { (0, PageSize), (PageSize, PageSize), (PageSize * 2, PageSize) }));
    }

    // The boundary case that a `page.Count < PageSize` check alone cannot end: the last full page is
    // followed by an empty one, and the empty response — not the page size — is what stops the loop.
    [Test]
    public async Task ExactMultipleOfPageSize_RequiresOneMoreRequestAndTerminatesOnTheEmptyPage()
    {
        RespondWith(PageSize, PageSize, 0);

        var drivers = await _reader.GetDriversAsync(CancellationToken.None);

        Assert.That(drivers, Has.Count.EqualTo(PageSize * 2));
        Assert.That(_requests, Has.Count.EqualTo(3), "An exact multiple needs a probe page to prove exhaustion.");
        Assert.That(_requests[^1].Skip, Is.EqualTo(PageSize * 2));
    }

    [Test]
    public async Task EmptyFirstPage_ReturnsNothingAndDoesNotPageAgain()
    {
        RespondWith(0);

        var drivers = await _reader.GetDriversAsync(CancellationToken.None);

        Assert.That(drivers, Is.Empty);
        Assert.That(_requests, Has.Count.EqualTo(1));
    }

    // A server that keeps returning full pages must not be followed forever: the loop is bounded by
    // MaxRows (100k), i.e. 200 full pages plus the one deliberate over-limit page that lets
    // ExcelHelper fail loudly instead of truncating.
    [Test]
    public async Task AlwaysFullPages_StopsAtTheRowLimitInsteadOfLoopingForever()
    {
        var requests = 0;
        _client
            .Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .Returns((GraphQLRequest request, CancellationToken _) =>
            {
                requests++;
                Assert.That(requests, Is.LessThanOrEqualTo(250), "FetchAllAsync is not bounded by the row limit.");
                return Task.FromResult(new GraphQLResponse<object> { Data = DriverPage(PageSize) });
            });

        var drivers = await _reader.GetDriversAsync(CancellationToken.None);

        // 100_000 / 500 = 200 pages reach the limit; the loop takes exactly one more so the caller
        // sees an over-limit dataset rather than a silently complete-looking one.
        Assert.That(requests, Is.EqualTo(201));
        Assert.That(drivers, Has.Count.EqualTo(100_500));
    }

    // Paging is shared by all three reads; prove the qualification and assignment entry points run
    // through the same loop rather than a one-shot fetch.
    [Test]
    public async Task QualificationsAndAssignments_PageThroughTheSameLoop()
    {
        var pages = new Queue<string>(
        [
            $"{{\"driverQualifications\":[{QualificationRows(PageSize)}]}}",
            "{\"driverQualifications\":[]}",
            $"{{\"driverAssignmentHistory\":[{AssignmentRows(3)}]}}"
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

        var qualifications = await _reader.GetDriverQualificationsAsync(null, 30, CancellationToken.None);
        Assert.That(qualifications, Has.Count.EqualTo(PageSize));
        Assert.That(_requests, Is.EqualTo(new[] { (0, PageSize), (PageSize, PageSize) }));

        _requests.Clear();
        var assignments = await _reader.GetDriverAssignmentHistoryAsync(null, null, null, null, CancellationToken.None);
        Assert.That(assignments, Has.Count.EqualTo(3));
        Assert.That(_requests, Is.EqualTo(new[] { (0, PageSize) }));
    }

    private static string QualificationRows(int count)
        => string.Join(',', Enumerable.Range(0, count).Select(i => $$"""
            {"driverQualificationId":"{{Guid.NewGuid()}}","driverId":"{{Guid.NewGuid()}}","driverName":"D{{i}}",
             "qualificationType":"License","category":"C2","number":"L-{{i}}","issuedAt":null,"expiresAt":null,
             "issuingAuthority":null,"status":"Valid"}
            """));

    private static string AssignmentRows(int count)
        => string.Join(',', Enumerable.Range(0, count).Select(i => $$"""
            {"driverId":"{{Guid.NewGuid()}}","driverName":"D{{i}}","transporterId":"{{Guid.NewGuid()}}",
             "transporterName":"T{{i}}","startsAt":"2026-07-01T00:00:00+00:00","endsAt":null,
             "assignmentType":"Regular","status":"Active","createdByPrincipal":"user:1"}
            """));
}
