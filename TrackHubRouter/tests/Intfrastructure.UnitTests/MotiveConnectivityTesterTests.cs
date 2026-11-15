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
using TrackHub.Router.Infrastructure.Motive.Models;

namespace TrackHub.Router.Infrastructure.Motive.Tests;

[TestFixture]
public class ConnectivityTesterTests
{
    private readonly CredentialTokenDto _credential = new(Guid.NewGuid(), "https://api.gomotive.com", "", "", "api-key-1", null, null, null, null, null);

    [Test]
    public async Task Ping_SendsApiKeyHeaderAndCallsVehicles()
    {
        using var httpClient = new HttpClient();
        var factory = new Mock<ICredentialHttpClientFactory>();
        factory.Setup(x => x.CreateClientAsync(_credential, It.IsAny<CancellationToken>())).Returns(httpClient);
        var service = new Mock<IHttpClientService>();
        service.Setup(x => x.GetAsync<VehiclesResponse>("v1/vehicles?per_page=1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VehiclesResponse([], null));

        await new ConnectivityTester(factory.Object, service.Object).Ping(_credential, CancellationToken.None);

        Assert.That(httpClient.DefaultRequestHeaders.GetValues("X-API-Key"), Is.EqualTo(new[] { "api-key-1" }));
        service.Verify(x => x.Init(httpClient, "Motive"), Times.Once);
        service.VerifyAll();
    }

    [Test]
    public async Task Ping_WithoutVehiclesList_Throws()
    {
        var factory = new Mock<ICredentialHttpClientFactory>();
        factory.Setup(x => x.CreateClientAsync(_credential, It.IsAny<CancellationToken>())).Returns(new HttpClient());
        var service = new Mock<IHttpClientService>();
        service.Setup(x => x.GetAsync<VehiclesResponse>(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VehiclesResponse(null, null));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await new ConnectivityTester(factory.Object, service.Object).Ping(_credential, CancellationToken.None));
    }
}
