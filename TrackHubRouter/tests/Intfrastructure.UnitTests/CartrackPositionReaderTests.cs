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

using System.Net;
using TrackHub.Router.Infrastructure.Cartrack.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Cartrack.Tests;

[TestFixture]
public class PositionReaderTests : PositionReaderTestsBase<PositionReader>
{
    private static readonly DateTimeOffset Time = new(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);

    protected override PositionReader CreatePositionReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService);

    private static VehicleStatus Status(string registration)
        => new(1, registration, Time, 0, 50, true, 0, 0, 0, 0, new StatusLocation(Time, 1, 2, null));

    private static VehicleEvent Event(DateTimeOffset time, int? gpsFixType = 3)
        => new(time, null, 1, 2, gpsFixType, 0, 0, null, null, null, null, null, null);

    private void SetupStatus(Page<VehicleStatus>? response)
        => HttpClientServiceMock.Setup(x => x.GetAsync<Page<VehicleStatus>>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(response);

    [Test]
    public async Task GetDevicePositionAsync_WithMultipleDevices_ReadsFleetOnceAndKeepsRequested()
    {
        SetupStatus(new Page<VehicleStatus>([Status("ABX123"), Status("ABX124"), Status("ZZZ999")], null));
        var devices = new List<DeviceTransporterVm>
        {
            CreateDeviceTransporterVm(1, "Truck-1", "ABX123"),
            CreateDeviceTransporterVm(2, "Truck-2", "ABX124")
        };

        var result = (await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken)).ToList();

        Assert.That(result.Select(p => p.DeviceName), Is.EquivalentTo(new[] { "Truck-1", "Truck-2" }));
        HttpClientServiceMock.Verify(x => x.GetAsync<Page<VehicleStatus>>("vehicles/status", null, TestCancellationToken), Times.Once);
    }

    [Test]
    public async Task GetDevicePositionAsync_SingleDevice_IgnoresPartialRegistrationMatches()
    {
        SetupStatus(new Page<VehicleStatus>([Status("ABX1234"), Status("ABX123")], null));

        var result = await PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(1, "Truck", "ABX123"), TestCancellationToken);

        Assert.That(result.DeviceName, Is.EqualTo("Truck"));
        HttpClientServiceMock.Verify(x => x.GetAsync<Page<VehicleStatus>>("vehicles/status?filter[registration]=ABX123", null, TestCancellationToken), Times.Once);
    }

    [Test]
    public async Task GetDevicePositionAsync_SingleDeviceOnlyPartialMatch_Throws()
    {
        SetupStatus(new Page<VehicleStatus>([Status("ABX1234")], null));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(1, "Truck", "ABX123"), TestCancellationToken));
    }

    [Test]
    public async Task GetDevicePositionAsync_WithoutDataList_Throws()
    {
        SetupStatus(new Page<VehicleStatus>(null, null));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await PositionReader.GetDevicePositionAsync([CreateDeviceTransporterVm(1, "A", "ABX123")], TestCancellationToken));
    }

    [Test]
    public async Task GetDevicePositionAsync_WhenRateLimited_PropagatesHttpError()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<Page<VehicleStatus>>(It.IsAny<string>(), null, TestCancellationToken))
            .ThrowsAsync(new HttpRequestException("Too Many Requests", null, HttpStatusCode.TooManyRequests));

        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await PositionReader.GetDevicePositionAsync([CreateDeviceTransporterVm(1, "A", "ABX123")], TestCancellationToken));
    }

    [Test]
    public async Task GetPositionAsync_PagesUntilLastPageAndTrimsToRange()
    {
        var from = Time;
        var to = Time.AddHours(2);
        HttpClientServiceMock.SetupSequence(x => x.GetAsync<Page<VehicleEvent>>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(new Page<VehicleEvent>([Event(from.AddSeconds(-1)), Event(from)], new PageMeta(1, 2)))
            .ReturnsAsync(new Page<VehicleEvent>([Event(from.AddHours(1), gpsFixType: 0), Event(to), Event(to.AddSeconds(1))], new PageMeta(2, 2)));

        var result = (await PositionReader.GetPositionAsync(from, to, CreateDeviceTransporterVm(1, "T", "ABX 123"), TestCancellationToken)).ToList();

        Assert.That(result.Select(p => p.DeviceDateTime), Is.EqualTo(new[] { from, to }));
        HttpClientServiceMock.Verify(x => x.GetAsync<Page<VehicleEvent>>(
            "vehicles/ABX%20123/events?start_timestamp=2026-03-09%2022%3A00%3A00&end_timestamp=2026-03-11%2004%3A00%3A00&page=1&limit=1000",
            null, TestCancellationToken), Times.Once);
        HttpClientServiceMock.Verify(x => x.GetAsync<Page<VehicleEvent>>(
            It.Is<string>(url => url.EndsWith("&page=2&limit=1000")), null, TestCancellationToken), Times.Once);
    }

    [Test]
    public async Task GetPositionAsync_WithoutDataList_Throws()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<Page<VehicleEvent>>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(new Page<VehicleEvent>(null, null));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await PositionReader.GetPositionAsync(Time.AddHours(-1), Time, CreateDeviceTransporterVm(1, "T", "ABX123"), TestCancellationToken));
    }

    [Test]
    public async Task GetPositionAsync_RangeBeyond31Days_IssuesOneRequestPerWindow()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<Page<VehicleEvent>>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(new Page<VehicleEvent>([], new PageMeta(1, 1)));

        await PositionReader.GetPositionAsync(Time, Time.AddDays(62).AddHours(1), CreateDeviceTransporterVm(1, "T", "ABX123"), TestCancellationToken);

        HttpClientServiceMock.Verify(x => x.GetAsync<Page<VehicleEvent>>(It.IsAny<string>(), null, TestCancellationToken), Times.Exactly(3));
    }

    [Test]
    public void EventWindows_SplitsAt31DaysWithoutOverlap()
    {
        var windows = PositionReader.EventWindows(Time, Time.AddDays(40)).ToList();

        Assert.That(windows, Is.EqualTo(new[]
        {
            (Time, Time + PositionReader.MaxEventWindow),
            (Time + PositionReader.MaxEventWindow + TimeSpan.FromSeconds(1), Time.AddDays(40)),
        }));
    }

    [Test]
    public void EventWindows_ExactlyOneWindow_YieldsSingleRange()
    {
        var end = Time + PositionReader.MaxEventWindow;

        Assert.That(PositionReader.EventWindows(Time, end).ToList(), Is.EqualTo(new[] { (Time, end) }));
    }

    [Test]
    public void FormatTimestamp_ConvertsToUtcWithoutOffset()
    {
        var value = new DateTimeOffset(2022, 1, 1, 10, 12, 2, TimeSpan.FromHours(2));

        Assert.That(Uri.UnescapeDataString(PositionReader.FormatTimestamp(value)), Is.EqualTo("2022-01-01 08:12:02"));
    }
}
