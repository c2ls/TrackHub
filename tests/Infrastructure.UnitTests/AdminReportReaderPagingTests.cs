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
using GraphQL;
using GraphQL.Client.Abstractions;
using Moq;
using Common.Application.Interfaces;
using TrackHub.Reporting.Infrastructure.GraphQLApi;

namespace TrackHub.Reporting.Infrastructure.UnitTests;

// Manager now answers accounts / groupsByAccount / usersByGroup / transportersByGroup with a page
// envelope. If this reader took only the first page, the group-membership export would lose entire
// GROUPS from its outer loop and the file would still look complete — no error, no short-row marker,
// just missing groups. These tests pin the drain: every page is followed, and an over-limit set
// raises instead of returning what fit.
[TestFixture]
public class AdminReportReaderPagingTests
{
    private const int PageSize = 500; // Manager's take clamp (PageRequest.MaxPageSize).

    private Mock<IGraphQLClient> _client = null!;
    private AdminReportReader _reader = null!;
    private List<(int Skip, int Take)> _requests = null!;

    [SetUp]
    public void SetUp()
    {
        _requests = [];
        _client = new Mock<IGraphQLClient>();

        var factory = new Mock<IGraphQLClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(_client.Object);

        _reader = new AdminReportReader(factory.Object);
    }

    private static (int Skip, int Take) ReadWindow(GraphQLRequest request)
    {
        var variables = request.Variables!;
        var type = variables.GetType();
        return ((int)type.GetProperty("skip")!.GetValue(variables)!,
                (int)type.GetProperty("take")!.GetValue(variables)!);
    }

    /// <summary>A <c>groupsByAccount</c> page envelope: camelCase items plus the unpaged total.</summary>
    private static JsonElement GroupPage(int count, int totalCount)
    {
        var json = new StringBuilder("{\"groupsByAccount\":{\"items\":[");
        for (var i = 0; i < count; i++)
        {
            if (i > 0)
            {
                json.Append(',');
            }

            json.Append(CultureInfo.InvariantCulture,
                $$"""{"groupId":{{i}},"name":"Group {{i}}","active":true,"accountId":"{{Guid.NewGuid()}}"}""");
        }

        json.Append(CultureInfo.InvariantCulture, $"],\"totalCount\":{totalCount}}}}}");
        return JsonDocument.Parse(json.ToString()).RootElement.Clone();
    }

    private void RespondWithGroupPages(params (int Count, int TotalCount)[] pages)
    {
        _client
            .Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .Returns((GraphQLRequest request, CancellationToken _) =>
            {
                _requests.Add(ReadWindow(request));
                Assert.That(_requests, Has.Count.LessThanOrEqualTo(pages.Length),
                    $"the drain issued request #{_requests.Count} but only {pages.Length} pages were scripted — the loop is not terminating.");

                var (count, totalCount) = pages[_requests.Count - 1];
                return Task.FromResult(new GraphQLResponse<object> { Data = GroupPage(count, totalCount) });
            });
    }

    [Test]
    public async Task SinglePartialPage_StopsAfterOneRequest()
    {
        RespondWithGroupPages((37, 37));

        var groups = await _reader.GetGroupsByAccountAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(groups, Has.Count.EqualTo(37));
            Assert.That(_requests, Is.EqualTo(new[] { (0, PageSize) }));
        });
    }

    // The regression this whole finding is about: an account with more groups than one page. Taking
    // the first page would silently drop the rest from the membership export's OUTER loop.
    [Test]
    public async Task MultiPageAccount_DrainsEveryGroupRatherThanTheFirstPage()
    {
        RespondWithGroupPages((PageSize, 1_150), (PageSize, 1_150), (150, 1_150));

        var groups = await _reader.GetGroupsByAccountAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(groups, Has.Count.EqualTo(1_150), "the export would have shipped without the missing groups");
            Assert.That(_requests, Is.EqualTo(new[] { (0, PageSize), (PageSize, PageSize), (PageSize * 2, PageSize) }));
        });
    }

    // An exact multiple of the page size: totalCount is what proves exhaustion, so no wasted probe.
    [Test]
    public async Task ExactMultipleOfPageSize_StopsOnTotalCountWithoutAnExtraRequest()
    {
        RespondWithGroupPages((PageSize, PageSize));

        var groups = await _reader.GetGroupsByAccountAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(groups, Has.Count.EqualTo(PageSize));
            Assert.That(_requests, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task EmptyFirstPage_ReturnsNothingAndDoesNotPageAgain()
    {
        RespondWithGroupPages((0, 0));

        var groups = await _reader.GetGroupsByAccountAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(groups, Is.Empty);
            Assert.That(_requests, Has.Count.EqualTo(1));
        });
    }

    // A set larger than a report may carry must RAISE. Returning the first 100k rows would be a
    // truncated export that nothing in the output identifies as truncated.
    [Test]
    public void OverTheRowLimit_ThrowsInsteadOfReturningATruncatedExport()
    {
        var requests = 0;
        _client
            .Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .Returns((GraphQLRequest request, CancellationToken _) =>
            {
                requests++;
                Assert.That(requests, Is.LessThanOrEqualTo(250), "the drain is not bounded by the row limit.");
                return Task.FromResult(new GraphQLResponse<object> { Data = GroupPage(PageSize, 500_000) });
            });

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            () => _reader.GetGroupsByAccountAsync(CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("groupsByAccount"));
    }
}
