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
using TrackHub.Router.Infrastructure.Motive.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Motive.Tests;

[TestFixture]
public class PositionReaderTests : PositionReaderTestsBase<PositionReader>
{
    private static readonly DateTimeOffset Time = new(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);

    protected override PositionReader CreatePositionReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService);

    private static Vehicle VehicleAt(long id, DateTimeOffset? time = null)
        => new(id, $"V-{id}", new VehicleLocation(time ?? Time, 1, 2, 0, 10, null, null, null));

    private void SetupVehicles(params VehiclesResponse?[] pages)
    {
        var sequence = HttpClientServiceMock.SetupSequence(x => x.GetAsync<VehiclesResponse>(It.IsAny<string>(), null, TestCancellationToken));
        foreach (var page in pages)
        {
            sequence = sequence.ReturnsAsync(page);
        }
    }

    [Test]
    public async Task GetDevicePositionAsync_WithMultipleDevices_ReturnsOnlyRequestedDevices()
    {
        var devices = new List<DeviceTransporterVm>
        {
            CreateDeviceTransporterVm(1, "Truck-1", "23"),
            CreateDeviceTransporterVm(2, "Truck-2", "24")
        };
        SetupVehicles(new VehiclesResponse([new(VehicleAt(23)), new(VehicleAt(24)), new(VehicleAt(99))], new Pagination(100, 1, 3)));

        var result = (await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken)).ToList();

        Assert.That(result.Select(p => p.DeviceName), Is.EquivalentTo(new[] { "Truck-1", "Truck-2" }));
        HttpClientServiceMock.Verify(x => x.GetAsync<VehiclesResponse>(
            "v1/vehicle_locations?vehicle_ids[]=23&vehicle_ids[]=24&per_page=100&page_no=1", null, TestCancellationToken), Times.Once);
    }

    [Test]
    public async Task GetDevicePositionAsync_With150Devices_SplitsIntoRequestsOf100Ids()
    {
        var devices = Enumerable.Range(1, 150).Select(i => CreateDeviceTransporterVm(i, $"T{i}", i.ToString())).ToList();
        var urls = new List<string>();
        HttpClientServiceMock.Setup(x => x.GetAsync<VehiclesResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .Callback<string, IDictionary<string, string>?, CancellationToken>((url, _, _) => urls.Add(url))
            .ReturnsAsync(new VehiclesResponse([], new Pagination(100, 1, 0)));

        await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken);

        Assert.That(urls, Has.Count.EqualTo(2));
        Assert.That(urls[0].Split("vehicle_ids[]=").Length - 1, Is.EqualTo(100));
        Assert.That(urls[1].Split("vehicle_ids[]=").Length - 1, Is.EqualTo(50));
    }

    [Test]
    public async Task GetDevicePositionAsync_WhenTotalExceedsPage_ReadsNextPage()
    {
        var devices = new List<DeviceTransporterVm> { CreateDeviceTransporterVm(1, "A", "1"), CreateDeviceTransporterVm(2, "B", "2") };
        SetupVehicles(
            new VehiclesResponse([new(VehicleAt(1))], new Pagination(1, 1, 2)),
            new VehiclesResponse([new(VehicleAt(2))], new Pagination(1, 2, 2)));

        var result = await PositionReader.GetDevicePositionAsync(devices, TestCancellationToken);

        Assert.That(result.Count(), Is.EqualTo(2));
        HttpClientServiceMock.Verify(x => x.GetAsync<VehiclesResponse>(
            It.Is<string>(url => url.EndsWith("page_no=2")), null, TestCancellationToken), Times.Once);
    }

    [Test]
    public async Task GetDevicePositionAsync_WithoutVehiclesList_Throws()
    {
        SetupVehicles(new VehiclesResponse(null, null));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await PositionReader.GetDevicePositionAsync([CreateDeviceTransporterVm(1, "A", "1")], TestCancellationToken));
    }

    [Test]
    public async Task GetDevicePositionAsync_WhenProviderRejects_PropagatesHttpError()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<VehiclesResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ThrowsAsync(new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized));

        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await PositionReader.GetDevicePositionAsync([CreateDeviceTransporterVm(1, "A", "1")], TestCancellationToken));
    }

    [Test]
    public async Task GetDevicePositionAsync_SingleDevice_ReturnsItsPosition()
    {
        SetupVehicles(new VehiclesResponse([new(VehicleAt(23))], null));

        var result = await PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(1, "Truck", "23"), TestCancellationToken);

        Assert.That(result.DeviceDateTime, Is.EqualTo(Time));
    }

    [Test]
    public async Task GetDevicePositionAsync_SingleDeviceWithoutLocation_Throws()
    {
        SetupVehicles(new VehiclesResponse([new(new Vehicle(23, "V", null))], null));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await PositionReader.GetDevicePositionAsync(CreateDeviceTransporterVm(1, "Truck", "23"), TestCancellationToken));
    }

    [Test]
    public async Task GetPositionAsync_RequestsCoveringDatesAndTrimsToRange()
    {
        var from = new DateTimeOffset(2026, 3, 10, 8, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 3, 11, 2, 0, 0, TimeSpan.Zero);
        VehicleLocationEntry At(DateTimeOffset t) => new(new VehicleLocation(t, 1, 2, 0, 0, null, null, null));
        HttpClientServiceMock.Setup(x => x.GetAsync<VehicleLocationsResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(new VehicleLocationsResponse([
                At(from.AddSeconds(-1)), At(from), At(from.AddHours(5)), At(to), At(to.AddSeconds(1))]));

        var result = (await PositionReader.GetPositionAsync(from, to, CreateDeviceTransporterVm(1, "T", "23"), TestCancellationToken)).ToList();

        Assert.That(result.Select(p => p.DeviceDateTime), Is.EqualTo(new[] { from, from.AddHours(5), to }));
        HttpClientServiceMock.Verify(x => x.GetAsync<VehicleLocationsResponse>(
            "v2/vehicle_locations/23?start_date=2026-03-10&end_date=2026-03-11&updated_after=2026-03-09&per_page=100&page_no=1", null, TestCancellationToken), Times.Once);
    }

    [Test]
    public async Task GetPositionAsync_PagesWhileTheResponseReportsMoreResults()
    {
        var from = new DateTimeOffset(2026, 3, 10, 8, 0, 0, TimeSpan.Zero);
        VehicleLocationEntry At(DateTimeOffset t) => new(new VehicleLocation(t, 1, 2, 0, 0, null, null, null));
        HttpClientServiceMock.Setup(x => x.GetAsync<VehicleLocationsResponse>(It.Is<string>(url => url.EndsWith("&page_no=1")), null, TestCancellationToken))
            .ReturnsAsync(new VehicleLocationsResponse([At(from)], new Pagination(100, 1, 101)));
        HttpClientServiceMock.Setup(x => x.GetAsync<VehicleLocationsResponse>(It.Is<string>(url => url.EndsWith("&page_no=2")), null, TestCancellationToken))
            .ReturnsAsync(new VehicleLocationsResponse([At(from.AddHours(1))], new Pagination(100, 2, 101)));

        var result = await PositionReader.GetPositionAsync(from, from.AddHours(2), CreateDeviceTransporterVm(1, "T", "23"), TestCancellationToken);

        Assert.That(result.Select(p => p.DeviceDateTime), Is.EqualTo(new[] { from, from.AddHours(1) }));
    }

    [Test]
    public async Task GetPositionAsync_RangeBeyondThreeMonths_IssuesOneRequestPerWindow()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<VehicleLocationsResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(new VehicleLocationsResponse([]));

        await PositionReader.GetPositionAsync(
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero),
            CreateDeviceTransporterVm(1, "T", "23"), TestCancellationToken);

        HttpClientServiceMock.Verify(x => x.GetAsync<VehicleLocationsResponse>(It.IsAny<string>(), null, TestCancellationToken), Times.Exactly(2));
    }

    [Test]
    public async Task GetPositionAsync_WithoutLocationList_Throws()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<VehicleLocationsResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(new VehicleLocationsResponse(null));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await PositionReader.GetPositionAsync(Time.AddHours(-1), Time, CreateDeviceTransporterVm(1, "T", "23"), TestCancellationToken));
    }

    [Test]
    public void DateWindows_SplitsAtThreeMonthsAndUsesUtcDates()
    {
        var from = new DateTimeOffset(2026, 1, 1, 0, 30, 0, TimeSpan.FromHours(2));
        var to = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero);

        var windows = PositionReader.DateWindows(from, to).ToList();

        Assert.That(windows, Is.EqualTo(new[]
        {
            (new DateOnly(2025, 12, 31), new DateOnly(2026, 3, 30)),
            (new DateOnly(2026, 3, 31), new DateOnly(2026, 4, 1)),
        }));
    }

    [Test]
    public void DateWindows_SameDay_YieldsSingleWindow()
    {
        var windows = PositionReader.DateWindows(Time.AddHours(-2), Time).ToList();

        Assert.That(windows, Is.EqualTo(new[] { (new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 10)) }));
    }
}
