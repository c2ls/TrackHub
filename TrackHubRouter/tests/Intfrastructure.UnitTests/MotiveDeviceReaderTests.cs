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

using TrackHub.Router.Infrastructure.Motive.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Motive.Tests;

[TestFixture]
public class DeviceReaderTests : DeviceReaderTestsBase<DeviceReader>
{
    protected override DeviceReader CreateDeviceReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService);

    [Test]
    public async Task GetDevicesAsync_All_PagesThroughVehicleList()
    {
        HttpClientServiceMock.SetupSequence(x => x.GetAsync<VehiclesResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(new VehiclesResponse([new(new Vehicle(1, "A", null)), new(new Vehicle(2, "B", null))], new Pagination(2, 1, 3)))
            .ReturnsAsync(new VehiclesResponse([new(new Vehicle(3, "C", null))], new Pagination(2, 2, 3)));

        var result = (await DeviceReader.GetDevicesAsync(TestCancellationToken)).ToList();

        Assert.That(result.Select(d => d.Serial), Is.EqualTo(new[] { "1", "2", "3" }));
        HttpClientServiceMock.Verify(x => x.GetAsync<VehiclesResponse>("v1/vehicles?per_page=100&page_no=1", null, TestCancellationToken), Times.Once);
        HttpClientServiceMock.Verify(x => x.GetAsync<VehiclesResponse>("v1/vehicles?per_page=100&page_no=2", null, TestCancellationToken), Times.Once);
    }

    [Test]
    public async Task GetDevicesAsync_All_StopsOnEmptyPage()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<VehiclesResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(new VehiclesResponse([], new Pagination(100, 1, 500)));

        var result = await DeviceReader.GetDevicesAsync(TestCancellationToken);

        AssertIsEmpty(result);
        HttpClientServiceMock.Verify(x => x.GetAsync<VehiclesResponse>(It.IsAny<string>(), null, TestCancellationToken), Times.Once);
    }

    [Test]
    public async Task GetDevicesAsync_All_WithoutVehiclesList_Throws()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<VehiclesResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync((VehiclesResponse?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await DeviceReader.GetDevicesAsync(TestCancellationToken));
    }

    [Test]
    public async Task GetDevicesAsync_Selected_ReturnsOnlyRequestedVehicles()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<VehiclesResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(new VehiclesResponse([new(new Vehicle(23, "V-23", null)), new(new Vehicle(99, "V-99", null))], null));

        var result = (await DeviceReader.GetDevicesAsync([CreateDeviceTransporterVm(1, "23"), CreateDeviceTransporterVm(2, "24")], TestCancellationToken)).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        AssertEquals(result[0], CreateExpectedDeviceVm(23, "23", "V-23"));
    }

    [Test]
    public async Task GetDeviceAsync_ReturnsMatchingVehicle()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<VehiclesResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(new VehiclesResponse([new(new Vehicle(23, "V-23", null))], null));

        var result = await DeviceReader.GetDeviceAsync(CreateDeviceTransporterVm(1, "23"), TestCancellationToken);

        AssertEquals(result, CreateExpectedDeviceVm(23, "23", "V-23"));
        HttpClientServiceMock.Verify(x => x.GetAsync<VehiclesResponse>(
            "v1/vehicle_locations?vehicle_ids[]=23&per_page=100&page_no=1", null, TestCancellationToken), Times.Once);
    }

    [Test]
    public async Task GetDeviceAsync_NotReturned_Throws()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<VehiclesResponse>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(new VehiclesResponse([], null));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await DeviceReader.GetDeviceAsync(CreateDeviceTransporterVm(1, "0"), TestCancellationToken));
    }
}
