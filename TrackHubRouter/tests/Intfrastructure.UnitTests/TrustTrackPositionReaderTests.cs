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

using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.TrustTrack.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.TrustTrack.Tests;

[TestFixture]
public class PositionReaderTests : PositionReaderTestsBase<PositionReader>
{
    private readonly CredentialTokenDto _credential = new(Guid.NewGuid(), "https://api.fm-track.com", "", "", "k&y", null, null, null, null, null);
    private HttpClient _httpClient = null!;

    protected override PositionReader CreatePositionReader(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService);

    [SetUp]
    public async Task SetUp()
    {
        _httpClient = new HttpClient();
        HttpClientFactoryMock.Setup(x => x.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(_httpClient);
        await PositionReader.Init(_credential, TestCancellationToken);
    }

    [TearDown]
    public void TearDown() => _httpClient.Dispose();

    private static TrackedObject Located(string id)
        => new(id, id, null, new Position(1, 2, new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero), 10, 0, null, null));

    [Test]
    public async Task GetDevicePositionAsync_ManyDevices_FollowsTheContinuationToken_AndSendsTheEscapedKey()
    {
        var firstPage = Enumerable.Range(1, TrustTrackReaderBase.PageLimit - 1).Select(i => Located($"x{i}")).Append(Located("a")).ToList();
        HttpClientServiceMock.Setup(x => x.GetAsync<ContinuationPage<TrackedObject>>(
                "objects-last-coordinate?version=3&limit=1000&api_key=k%26y", It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContinuationPage<TrackedObject>("2026-03-01T10:00:00Z", firstPage, null));
        HttpClientServiceMock.Setup(x => x.GetAsync<ContinuationPage<TrackedObject>>(
                "objects-last-coordinate?version=3&limit=1000&continuation_token=2026-03-01T10%3A00%3A00Z&api_key=k%26y", It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContinuationPage<TrackedObject>(null, [Located("b")], null));
        var devices = new[] { CreateDeviceTransporterVm(serial: "a"), CreateDeviceTransporterVm(serial: "b") };

        var positions = (await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken)).ToList();

        Assert.That(positions.Select(p => p.TransporterId), Is.EquivalentTo(devices.Select(d => d.TransporterId)));
    }

    [Test]
    public async Task GetPositionAsync_ReadsTheObjectHistory_AndKeepsTheRequestedRange()
    {
        var from = new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);
        HttpClientServiceMock.Setup(x => x.GetAsync<ContinuationPage<Coordinate>>(
                "objects/a/coordinates?version=3&from_datetime=2026-03-01T10%3A00%3A00.000Z&to_datetime=2026-03-01T11%3A00%3A00.000Z&limit=1000&api_key=k%26y",
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContinuationPage<Coordinate>(null, null,
            [
                new Coordinate(from.AddMinutes(5), false, new Position(1, 2, null, 0, 0, null, null), null, null),
                new Coordinate(from.AddHours(2), true, new Position(1, 2, null, 0, 0, null, null), null, null)
            ]));

        var positions = (await PositionReader.GetPositionAsync(from, from.AddHours(1), CreateDeviceTransporterVm(serial: "a"), TestCancellationToken)).ToList();

        Assert.That(positions.Single().DeviceDateTime, Is.EqualTo(from.AddMinutes(5)));
    }

    [Test]
    public async Task GetDevicePositionAsync_ObjectWithoutCoordinate_Throws()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<TrackedObject>("objects-last-coordinate/a?version=3&api_key=k%26y", It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TrackedObject("a", null, null, null));

        await Assert.ThrowsAsync<InvalidOperationException>(() => PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(serial: "a"), TestCancellationToken));
    }

    [Test]
    public async Task GetDevicePositionAsync_ShortPagesKeepPaging_UntilAnEmptyPage()
    {
        const string first = "objects-last-coordinate?version=3&limit=1000&api_key=k%26y";
        const string second = "objects-last-coordinate?version=3&limit=1000&continuation_token=t1&api_key=k%26y";
        const string third = "objects-last-coordinate?version=3&limit=1000&continuation_token=t2&api_key=k%26y";
        HttpClientServiceMock.Setup(x => x.GetAsync<ContinuationPage<TrackedObject>>(first, It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContinuationPage<TrackedObject>("t1", [Located("a")], null));
        HttpClientServiceMock.Setup(x => x.GetAsync<ContinuationPage<TrackedObject>>(second, It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContinuationPage<TrackedObject>("t2", [Located("b")], null));
        HttpClientServiceMock.Setup(x => x.GetAsync<ContinuationPage<TrackedObject>>(third, It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContinuationPage<TrackedObject>("t3", [], null));
        var devices = new[] { CreateDeviceTransporterVm(serial: "a"), CreateDeviceTransporterVm(serial: "b") };

        var positions = (await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken)).ToList();

        Assert.That(positions, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetDevicePositionAsync_TokenThatDoesNotAdvance_Throws()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<ContinuationPage<TrackedObject>>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContinuationPage<TrackedObject>("stuck", [Located("a")], null));

        await Assert.ThrowsAsync<InvalidOperationException>(() => PositionReader.GetDevicePositionAsync([CreateDeviceTransporterVm(serial: "a")], TestCancellationToken));
    }
}
