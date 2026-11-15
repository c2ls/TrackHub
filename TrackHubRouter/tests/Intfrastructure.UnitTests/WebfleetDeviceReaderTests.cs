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
public class DeviceReaderTests : DeviceReaderTestsBase<DeviceReader>
{
    private readonly List<string> _urls = [];

    private const string Objects = """
        [{"objectno":"A1","objectname":"Alpha","objectuid":"1-a"},
         {"objectno":"B2","objectname":"Bravo","deleted":true},
         {"objectno":"C3"}]
        """;

    protected override DeviceReader CreateDeviceReader(
        ICredentialHttpClientFactory httpClientFactory,
        IHttpClientService httpClientService)
        => new(httpClientFactory, httpClientService);

    private async Task InitAsync(string body)
    {
        _urls.Clear();
        HttpClientFactoryMock.Setup(x => x.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .Returns(new HttpClient());
        HttpClientServiceMock.Setup(x => x.GetAsync<JsonElement>(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .Callback<string, IDictionary<string, string>?, CancellationToken>((url, _, _) => _urls.Add(url))
            .ReturnsAsync(() => JsonDocument.Parse(body).RootElement.Clone());
        await DeviceReader.Init(
            new CredentialTokenDto(Guid.NewGuid(), "https://csv.webfleet.com/extern", "user", "pass", "key", "acct", null, null, null, null),
            TestCancellationToken);
    }

    [Test]
    public async Task GetDevicesAsync_ReturnsCatalogWithoutDeletedObjects()
    {
        await InitAsync(Objects);

        var result = (await DeviceReader.GetDevicesAsync(TestCancellationToken)).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Select(d => d.Serial), Is.EqualTo(new[] { "A1", "C3" }));
            AssertEquals(result[0], CreateExpectedDeviceVm(serial: "A1", name: "Alpha") with { DeviceId = null });
            Assert.That(result[1].Name, Is.EqualTo("C3"));
            Assert.That(_urls.Single(), Does.EndWith("&action=showObjectReportExtern"));
        }
    }

    [Test]
    public async Task GetDevicesAsync_ForRequestedDevices_ReturnsOnlyThoseDevices()
    {
        await InitAsync(Objects);
        var requested = new[] { CreateDeviceTransporterVm(7, "C3"), CreateDeviceTransporterVm(8, "Z9") };

        var result = (await DeviceReader.GetDevicesAsync(requested, TestCancellationToken)).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Serial, Is.EqualTo("C3"));
            Assert.That(result[0].Identifier, Is.EqualTo(7));
        }
    }

    [Test]
    public async Task GetDeviceAsync_QueriesByObjectNumber()
    {
        await InitAsync(Objects);

        var result = await DeviceReader.GetDeviceAsync(CreateDeviceTransporterVm(0, "A1"), TestCancellationToken);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name, Is.EqualTo("Alpha"));
            Assert.That(_urls.Single(), Does.EndWith("&action=showObjectReportExtern&objectno=A1"));
        }
    }

    [Test]
    public async Task GetDeviceAsync_ForUnknownDevice_Throws()
    {
        await InitAsync("[]");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => DeviceReader.GetDeviceAsync(CreateDeviceTransporterVm(0, "A1"), TestCancellationToken));
    }

    [Test]
    public async Task GetDevicesAsync_WithErrorPayload_ThrowsInsteadOfReturningEmpty()
    {
        await InitAsync("""{"errorCode":8014,"errorMsg":"External requests are not allowed"}""");

        var exception = await Assert.ThrowsAsync<WebfleetApiException>(() => DeviceReader.GetDevicesAsync(TestCancellationToken));

        Assert.That(exception!.Code, Is.EqualTo(8014));
    }

    [Test]
    public async Task Init_WithoutAccountName_Throws()
    {
        HttpClientFactoryMock.Setup(x => x.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .Returns(new HttpClient());

        await Assert.ThrowsAsync<InvalidOperationException>(() => DeviceReader.Init(
            new CredentialTokenDto(Guid.NewGuid(), "https://csv.webfleet.com/extern", "user", "pass", "key", null, null, null, null, null),
            TestCancellationToken));
    }
}
