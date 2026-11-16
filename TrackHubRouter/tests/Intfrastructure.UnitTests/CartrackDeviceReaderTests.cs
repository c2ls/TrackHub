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

using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.Cartrack.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Cartrack.Tests;

[TestFixture]
public class DeviceReaderTests : DeviceReaderTestsBase<DeviceReader>
{
    protected override DeviceReader CreateDeviceReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService);

    private void SetupVehicles(Page<Vehicle>? response)
        => HttpClientServiceMock.Setup(x => x.GetAsync<Page<Vehicle>>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(response);

    [Test]
    public async Task GetDevicesAsync_All_PagesUntilLastPage()
    {
        HttpClientServiceMock.SetupSequence(x => x.GetAsync<Page<Vehicle>>(It.IsAny<string>(), null, TestCancellationToken))
            .ReturnsAsync(new Page<Vehicle>([new(1, "A", null)], new PageMeta(1, 2)))
            .ReturnsAsync(new Page<Vehicle>([new(2, "B", null)], new PageMeta(2, 2)));

        var result = (await DeviceReader.GetDevicesAsync(TestCancellationToken)).ToList();

        Assert.That(result.Select(d => d.Serial), Is.EqualTo(new[] { "A", "B" }));
        HttpClientServiceMock.Verify(x => x.GetAsync<Page<Vehicle>>("vehicles?page=1&limit=1000", null, TestCancellationToken), Times.Once);
        HttpClientServiceMock.Verify(x => x.GetAsync<Page<Vehicle>>("vehicles?page=2&limit=1000", null, TestCancellationToken), Times.Once);
    }

    [Test]
    public async Task GetDevicesAsync_All_WithoutDataList_Throws()
    {
        SetupVehicles(null);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await DeviceReader.GetDevicesAsync(TestCancellationToken));
    }

    [Test]
    public async Task GetDevicesAsync_Selected_FiltersByRegistrationsInChunksOf100()
    {
        var devices = Enumerable.Range(1, 150).Select(i => CreateDeviceTransporterVm(i, $"R{i}")).ToList();
        var urls = new List<string>();
        HttpClientServiceMock.Setup(x => x.GetAsync<Page<Vehicle>>(It.IsAny<string>(), null, TestCancellationToken))
            .Callback<string, IDictionary<string, string>?, CancellationToken>((url, _, _) => urls.Add(url))
            .ReturnsAsync(new Page<Vehicle>([new(1, "R1", "One"), new(999, "R999", null)], new PageMeta(1, 1)));

        var result = (await DeviceReader.GetDevicesAsync(devices, TestCancellationToken)).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(urls, Has.Count.EqualTo(2));
        Assert.That(urls[0], Does.StartWith("vehicles?filter[registrations]=R1,R2,"));
        Assert.That(urls[0].Split(',').Length, Is.EqualTo(100));
        Assert.That(urls[1].Split(',').Length, Is.EqualTo(50));
    }

    [Test]
    public async Task GetDeviceAsync_ReturnsExactRegistration()
    {
        SetupVehicles(new Page<Vehicle>([new(654321, "ABX123", "Big car")], null));

        var result = await DeviceReader.GetDeviceAsync(CreateDeviceTransporterVm(1, "ABX123"), TestCancellationToken);

        AssertEquals(result, CreateExpectedDeviceVm(654321, "ABX123", "Big car"));
        HttpClientServiceMock.Verify(x => x.GetAsync<Page<Vehicle>>(
            "vehicles?filter[registrations]=ABX123&page=1&limit=1000", null, TestCancellationToken), Times.Once);
    }

    [Test]
    public async Task GetDeviceAsync_NotReturned_Throws()
    {
        SetupVehicles(new Page<Vehicle>([], null));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await DeviceReader.GetDeviceAsync(CreateDeviceTransporterVm(1, "ABX123"), TestCancellationToken));
    }

    [Test]
    public void BasicAuthorization_EncodesUsernameAndPassword()
    {
        var header = BasicAuthentication.Header("user", "pass");

        Assert.Multiple(() =>
        {
            Assert.That(header.Scheme, Is.EqualTo("Basic"));
            Assert.That(header.Parameter, Is.EqualTo("dXNlcjpwYXNz"));
        });
    }

    [Test]
    public async Task Ping_SetsBasicAuthAndReadsOneVehicle()
    {
        var credential = new CredentialTokenDto(Guid.NewGuid(), "https://fleetapi-za.cartrack.com/rest", "user", "pass", null, null, null, null, null, null);
        using var httpClient = new HttpClient();
        HttpClientFactoryMock.Setup(x => x.CreateClientAsync(credential, It.IsAny<CancellationToken>())).Returns(httpClient);
        HttpClientServiceMock.Setup(x => x.GetAsync<Page<Vehicle>>("vehicles?limit=1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Page<Vehicle>([], null));

        await new ConnectivityTester(HttpClientFactoryMock.Object, HttpClientServiceMock.Object).Ping(credential, TestCancellationToken);

        Assert.That(httpClient.DefaultRequestHeaders.Authorization!.Parameter, Is.EqualTo("dXNlcjpwYXNz"));
        HttpClientServiceMock.Verify(x => x.Init(httpClient, "Cartrack"), Times.Once);
        HttpClientServiceMock.VerifyAll();
    }

    [Test]
    public async Task Ping_WithoutDataList_Throws()
    {
        var credential = new CredentialTokenDto(Guid.NewGuid(), "https://fleetapi-za.cartrack.com/rest", "user", "pass", null, null, null, null, null, null);
        HttpClientFactoryMock.Setup(x => x.CreateClientAsync(credential, It.IsAny<CancellationToken>())).Returns(new HttpClient());
        HttpClientServiceMock.Setup(x => x.GetAsync<Page<Vehicle>>(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Page<Vehicle>(null, null));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await new ConnectivityTester(HttpClientFactoryMock.Object, HttpClientServiceMock.Object).Ping(credential, TestCancellationToken));
    }
}
