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
using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Webfleet.Tests;

[TestFixture]
public class PositionReaderTests : PositionReaderTestsBase<PositionReader>
{
    private readonly List<string> _urls = [];
    private HttpClient _httpClient = null!;

    [TearDown]
    public void TearDown() => _httpClient?.Dispose();

    protected override PositionReader CreatePositionReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService);

    private static CredentialTokenDto Credential() => new(
        Guid.NewGuid(), "https://csv.webfleet.com/extern", "wf user", "p@ss", "api-key", "acct ü", null, null, null, null);

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private async Task InitAsync(params string[] bodies)
    {
        _urls.Clear();
        _httpClient = new HttpClient();
        HttpClientFactoryMock.Setup(x => x.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .Returns(_httpClient);
        var queue = new Queue<string>(bodies);
        HttpClientServiceMock.Setup(x => x.GetAsync<JsonElement>(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .Callback<string, IDictionary<string, string>?, CancellationToken>((url, _, _) => _urls.Add(url))
            .ReturnsAsync(() => Json(queue.Count > 1 ? queue.Dequeue() : queue.Peek()));
        await PositionReader.Init(Credential(), TestCancellationToken);
    }

    private const string TwoObjects = """
        [{"objectno":"A1","objectname":"Alpha","pos_time":"2026-07-17T09:20:00Z","latitude_mdeg":4710989,"longitude_mdeg":-74072092,"speed":"40"},
         {"objectno":"B2","objectname":"Bravo","pos_time":"2026-07-17T09:21:00Z","latitude_mdeg":4720000,"longitude_mdeg":-74080000},
         {"objectno":"C3","objectname":"No fix"}]
        """;

    [Test]
    public async Task Init_UsesBasicAuthAndKeepsCredentialsOutOfTheUrl()
    {
        await InitAsync(TwoObjects);

        await PositionReader.GetDevicePositionAsync([CreateDeviceTransporterVm(serial: "A1")], TestCancellationToken);

        var url = _urls.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization!.Scheme, Is.EqualTo("Basic"));
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization!.Parameter, Is.EqualTo("d2YgdXNlcjpwQHNz"));
            Assert.That(url, Does.StartWith("https://csv.webfleet.com/extern?account=acct%20%C3%BC&apikey=api-key&"));
            Assert.That(url, Does.Contain("&lang=en&outputformat=json&useISO8601=true&useUTF8=true&action=showObjectReportExtern"));
            Assert.That(url, Does.Not.Contain("username").And.Not.Contain("password").And.Not.Contain("p%40ss"));
        }
    }

    [Test]
    public async Task GetDevicePositionAsync_WithManyDevices_ReturnsOnlyRequestedDevicesWithAFix()
    {
        await InitAsync(TwoObjects);
        var devices = new[]
        {
            CreateDeviceTransporterVm(name: "Alpha", serial: "A1"),
            CreateDeviceTransporterVm(name: "No fix", serial: "C3"),
            CreateDeviceTransporterVm(name: "Missing", serial: "Z9")
        };

        var result = (await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken)).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].TransporterId, Is.EqualTo(devices[0].TransporterId));
            Assert.That(result[0].Latitude, Is.EqualTo(4.710989).Within(1e-9));
            Assert.That(result[0].Speed, Is.EqualTo(40));
            Assert.That(_urls, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task GetDevicePositionAsync_WithErrorPayload_Throws()
    {
        await InitAsync("""{"errorCode":8011,"errorMsg":"request quota reached"}""");

        var exception = await Assert.ThrowsAsync<WebfleetApiException>(
            () => PositionReader.GetDevicePositionAsync([CreateDeviceTransporterVm(serial: "A1")], TestCancellationToken));

        Assert.That(exception!.Code, Is.EqualTo(8011));
    }

    [Test]
    public async Task GetDevicePositionAsync_ForOneDevice_FiltersByObjectNumber()
    {
        await InitAsync(TwoObjects);
        var device = CreateDeviceTransporterVm(name: "Bravo", serial: "B2");

        var result = await PositionReader.GetDevicePositionAsync(device, TestCancellationToken);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_urls.Single(), Does.EndWith("&action=showObjectReportExtern&objectno=B2"));
            Assert.That(result.TransporterId, Is.EqualTo(device.TransporterId));
            Assert.That(result.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2026, 7, 17, 9, 21, 0, TimeSpan.Zero)));
        }
    }

    [Test]
    public async Task GetDevicePositionAsync_ForUnknownDevice_Throws()
    {
        await InitAsync("[]");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(serial: "A1"), TestCancellationToken));
    }

    [Test]
    public async Task GetDevicePositionAsync_ForADeviceWithoutAPosition_Throws()
    {
        await InitAsync(TwoObjects);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(serial: "C3"), TestCancellationToken));
    }

    [TestCase(20, false)]
    [TestCase(22, true)]
    public async Task GetPositionAsync_SpacesRequestsOnlyWhenTheRangeExceedsTheMinuteQuota(int days, bool spaced)
    {
        await InitAsync("[]");
        var from = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var spacing = PositionReader.TrackRequestSpacing;
        PositionReader.TrackRequestSpacing = TimeSpan.FromMilliseconds(20);
        var started = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await PositionReader.GetPositionAsync(from, from.AddDays(days), CreateDeviceTransporterVm(serial: "A1"), TestCancellationToken);
        }
        finally
        {
            PositionReader.TrackRequestSpacing = spacing;
        }

        Assert.That(started.ElapsedMilliseconds >= 10 * 20, Is.EqualTo(spaced));
    }

    [Test]
    public async Task GetPositionAsync_SkipsPointsWithoutCoordinates()
    {
        await InitAsync("""[{"pos_time":"2026-07-10T08:00:00Z","speed":10},{"pos_time":"2026-07-10T09:00:00Z","latitude":4710989,"longitude":-74072092,"speed":20}]""");
        var from = new DateTimeOffset(2026, 7, 10, 0, 0, 0, TimeSpan.Zero);

        var result = await PositionReader.GetPositionAsync(from, from.AddDays(1), CreateDeviceTransporterVm(serial: "A1"), TestCancellationToken);

        Assert.That(result.Select(p => p.Speed), Is.EqualTo(new[] { 20d }));
    }

    [Test]
    public async Task GetPositionAsync_SplitsRangeIntoTwoDayWindowsAndKeepsPointsInsideTheRange()
    {
        await InitAsync(
            """[{"pos_time":"2026-07-09T23:59:59Z","latitude":4710989,"longitude":-74072092,"speed":10}]""",
            """[{"pos_time":"2026-07-12T08:00:00Z","latitude":4710989,"longitude":-74072092,"speed":20}]""",
            """[{"pos_time":"2026-07-14T08:00:00Z","latitude":4710989,"longitude":-74072092,"speed":30},{"pos_time":"2026-07-15T06:00:01Z","latitude":4710989,"longitude":-74072092,"speed":40}]""");
        var from = new DateTimeOffset(2026, 7, 10, 1, 0, 0, TimeSpan.FromHours(1));
        var to = new DateTimeOffset(2026, 7, 15, 6, 0, 0, TimeSpan.Zero);

        var result = (await PositionReader.GetPositionAsync(from, to, CreateDeviceTransporterVm(serial: "A 1"), TestCancellationToken)).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_urls, Has.Count.EqualTo(3));
            Assert.That(_urls[0], Does.EndWith("&action=showTracks&objectno=A%201&range_pattern=ud&rangefrom_string=2026-07-10T00:00:00Z&rangeto_string=2026-07-12T00:00:00Z"));
            Assert.That(_urls[1], Does.EndWith("&rangefrom_string=2026-07-12T00:00:00Z&rangeto_string=2026-07-14T00:00:00Z"));
            Assert.That(_urls[2], Does.EndWith("&rangefrom_string=2026-07-14T00:00:00Z&rangeto_string=2026-07-15T06:00:00Z"));
            Assert.That(result.Select(p => p.Speed), Is.EqualTo(new[] { 20d, 30d }));
        }
    }

    [Test]
    public async Task GetPositionAsync_WithErrorInALaterWindow_Throws()
    {
        await InitAsync("[]", """{"errorCode":47,"errorMsg":"invalid time period"}""");

        var exception = await Assert.ThrowsAsync<WebfleetApiException>(() => PositionReader.GetPositionAsync(
            DateTimeOffset.UtcNow.AddDays(-3), DateTimeOffset.UtcNow, CreateDeviceTransporterVm(serial: "A1"), TestCancellationToken));

        Assert.That(exception!.Code, Is.EqualTo(47));
    }

    [Test]
    public void SplitRange_WithExactlyTwoDays_UsesOneWindow()
    {
        var from = new DateTimeOffset(2026, 7, 10, 0, 0, 0, TimeSpan.Zero);

        var windows = PositionReader.SplitRange(from, from.AddDays(2)).ToList();

        Assert.That(windows, Is.EqualTo(new[] { (from, from.AddDays(2)) }));
    }

    [Test]
    public void SplitRange_JustOverTwoDays_UsesTwoContiguousWindows()
    {
        var from = new DateTimeOffset(2026, 7, 10, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddDays(2).AddSeconds(1);

        var windows = PositionReader.SplitRange(from, to).ToList();

        Assert.That(windows, Is.EqualTo(new[] { (from, from.AddDays(2)), (from.AddDays(2), to) }));
    }

    [Test]
    public void SplitRange_WithEmptyRange_HasNoWindows()
    {
        var from = DateTimeOffset.UtcNow;

        Assert.That(PositionReader.SplitRange(from, from), Is.Empty);
    }

    [Test]
    public void FormatRangeBound_WritesUtcIso8601()
        => Assert.That(
            PositionReader.FormatRangeBound(new DateTimeOffset(2026, 7, 10, 7, 5, 9, 500, TimeSpan.FromHours(-5))),
            Is.EqualTo("2026-07-10T12:05:09Z"));
}
