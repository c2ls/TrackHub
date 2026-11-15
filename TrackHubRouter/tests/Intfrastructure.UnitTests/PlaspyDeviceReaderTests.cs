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

using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.Plaspy.Models;
using TrackHub.Router.Infrastructure.Tests;

namespace TrackHub.Router.Infrastructure.Plaspy.Tests;

[TestFixture]
public class DeviceReaderTests : DeviceReaderTestsBase<DeviceReader>
{
    protected override DeviceReader CreateDeviceReader(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService, new Mock<ICredentialWriter>().Object, SessionStoreMock.Object);

    [SetUp]
    public async Task SetUp()
    {
        HttpClientFactoryMock.Setup(x => x.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(() => new HttpClient());
        HttpClientServiceMock.Setup(x => x.PostAsync<AuthResult>("api/Auth/Token", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResult(true, null, null, null, "jwt", "2099-01-01T00:00:00Z"));
        await DeviceReader.Init(new CredentialTokenDto(Guid.NewGuid(), "https://api.plaspy.com", "user", "", "key", null, null, null, null, null), TestCancellationToken);
    }

    private void SetupDevices(DevicesResponse response)
        => HttpClientServiceMock.Setup(x => x.GetAsync<DevicesResponse>("api/devices", It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

    [Test]
    public async Task GetDevicesAsync_ReturnsEveryDevice()
    {
        SetupDevices(new DevicesResponse(true, null, [new Device("dev-1", "Van"), new Device("dev-2", "Truck")]));

        var devices = await DeviceReader.GetDevicesAsync(TestCancellationToken);

        Assert.That(devices.Select(d => d.Serial), Is.EqualTo(new[] { "dev-1", "dev-2" }));
    }

    [Test]
    public async Task GetDevicesAsync_ForDevices_ReturnsOnlyTheRequestedOnes()
    {
        SetupDevices(new DevicesResponse(true, null, [new Device("dev-1", "Van"), new Device("dev-2", "Truck")]));

        var devices = await DeviceReader.GetDevicesAsync([CreateDeviceTransporterVm(0, "dev-2")], TestCancellationToken);

        Assert.That(devices.Single().Serial, Is.EqualTo("dev-2"));
    }

    [Test]
    public async Task GetDevicesAsync_ErrorPayload_Throws()
    {
        SetupDevices(new DevicesResponse(false, "API not enabled", null));

        await Assert.ThrowsAsync<InvalidOperationException>(() => DeviceReader.GetDevicesAsync(TestCancellationToken));
    }

    [Test]
    public async Task GetDeviceAsync_MissingDevice_Throws()
    {
        HttpClientServiceMock.Setup(x => x.GetAsync<DeviceResponse>("api/devices/dev-9", It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeviceResponse(true, null, null));

        await Assert.ThrowsAsync<InvalidOperationException>(() => DeviceReader.GetDeviceAsync(CreateDeviceTransporterVm(0, "dev-9"), TestCancellationToken));
    }
}
