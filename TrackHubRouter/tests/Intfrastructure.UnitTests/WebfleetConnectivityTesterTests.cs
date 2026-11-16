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

namespace TrackHub.Router.Infrastructure.Webfleet.Tests;

[TestFixture]
public class ConnectivityTesterTests
{
    private Mock<ICredentialHttpClientFactory> _httpClientFactory = null!;
    private Mock<IHttpClientService> _httpClientService = null!;

    private static CredentialTokenDto Credential(Guid id) => new(
        id, "https://csv.webfleet.com/extern", "user", "pass", "key", "acct", null, null, null, null);

    [SetUp]
    public void SetUp()
    {
        _httpClientFactory = new Mock<ICredentialHttpClientFactory>();
        _httpClientFactory.Setup(x => x.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .Returns(() => new HttpClient());
        _httpClientService = new Mock<IHttpClientService>();
    }

    [Test]
    public async Task Ping_CallsShowSettingsEveryTime()
    {
        _httpClientService.Setup(x => x.GetAsync<JsonElement>(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => JsonDocument.Parse("""[{"od_weight_idling":25}]""").RootElement.Clone());
        var tester = new ConnectivityTester(_httpClientFactory.Object, _httpClientService.Object);
        var credential = Credential(Guid.NewGuid());

        await tester.Ping(credential, CancellationToken.None);
        await tester.Ping(credential, CancellationToken.None);

        _httpClientService.Verify(x => x.GetAsync<JsonElement>(
            It.Is<string>(url => url.EndsWith("&action=showSettings")), null, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task Ping_WithAuthenticationError_Throws()
    {
        _httpClientService.Setup(x => x.GetAsync<JsonElement>(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => JsonDocument.Parse("""{"errorCode":1106,"errorMsg":"Authentication failed"}""").RootElement.Clone());
        var tester = new ConnectivityTester(_httpClientFactory.Object, _httpClientService.Object);

        var exception = await Assert.ThrowsAsync<WebfleetApiException>(() => tester.Ping(Credential(Guid.NewGuid()), CancellationToken.None));

        Assert.That(exception!.Code, Is.EqualTo(1106));
    }

    [Test]
    public async Task Requests_ForOneCredential_NeverOverlapAcrossReaders()
    {
        var inFlight = 0;
        var maxInFlight = 0;
        _httpClientService.Setup(x => x.GetAsync<JsonElement>(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                var current = Interlocked.Increment(ref inFlight);
                InterlockedMax(ref maxInFlight, current);
                await Task.Delay(20);
                Interlocked.Decrement(ref inFlight);
                return JsonDocument.Parse("[]").RootElement.Clone();
            });
        var credential = Credential(Guid.NewGuid());
        var testers = Enumerable.Range(0, 4)
            .Select(_ => new ConnectivityTester(_httpClientFactory.Object, _httpClientService.Object))
            .ToList();

        await Task.WhenAll(testers.Select(t => t.Ping(credential, CancellationToken.None)));

        Assert.That(maxInFlight, Is.EqualTo(1));
    }

    [Test]
    public async Task Requests_ForDifferentCredentials_RunConcurrently()
    {
        var inFlight = 0;
        var maxInFlight = 0;
        var release = new TaskCompletionSource();
        _httpClientService.Setup(x => x.GetAsync<JsonElement>(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                InterlockedMax(ref maxInFlight, Interlocked.Increment(ref inFlight));
                if (Volatile.Read(ref inFlight) == 2)
                {
                    release.TrySetResult();
                }
                await release.Task.WaitAsync(TimeSpan.FromSeconds(5));
                return JsonDocument.Parse("[]").RootElement.Clone();
            });

        await Task.WhenAll(
            new ConnectivityTester(_httpClientFactory.Object, _httpClientService.Object).Ping(Credential(Guid.NewGuid()), CancellationToken.None),
            new ConnectivityTester(_httpClientFactory.Object, _httpClientService.Object).Ping(Credential(Guid.NewGuid()), CancellationToken.None));

        Assert.That(maxInFlight, Is.EqualTo(2));
    }

    private static void InterlockedMax(ref int target, int value)
    {
        int current;
        while (value > (current = Volatile.Read(ref target)) && Interlocked.CompareExchange(ref target, value, current) != current)
        {
        }
    }
}
