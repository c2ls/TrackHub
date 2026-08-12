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
using TrackHub.Router.Infrastructure.Common;

namespace TrackHub.Router.Infrastructure.Tests;

[TestFixture]
public class CredentialHttpClientFactoryTests
{
    private CredentialHttpClientFactory _factory;

    private static CredentialTokenDto CredentialWith(string uri)
        => new(Guid.NewGuid(), uri, "user", "secret", null, null, "api-token", null, null, null);

    [SetUp]
    public void Setup()
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(f => f.CreateClient(CredentialHttpClientFactory.ProviderHttpClientName))
            .Returns(() => new HttpClient());
        _factory = new CredentialHttpClientFactory(httpClientFactory.Object);
    }

    // The credential Uri is operator-entered text and nothing rejects a missing trailing slash,
    // so the factory owns the invariant the relative-path providers depend on.
    [TestCase("https://gps.example.com/gateway", "https://gps.example.com/gateway/")]
    [TestCase("https://gps.example.com", "https://gps.example.com/")]
    [TestCase("https://gps.example.com:8443/a/b", "https://gps.example.com:8443/a/b/")]
    public void CreateClientAsync_AppendsAMissingTrailingSlashToTheBaseAddress(string uri, string expected)
    {
        var client = _factory.CreateClientAsync(CredentialWith(uri), CancellationToken.None);

        Assert.That(client.BaseAddress?.ToString(), Is.EqualTo(expected));
    }

    [TestCase("https://gps.example.com/")]
    [TestCase("https://gps.example.com/gateway/")]
    public void CreateClientAsync_LeavesAnAlreadySlashTerminatedUriAlone(string uri)
    {
        var client = _factory.CreateClientAsync(CredentialWith(uri), CancellationToken.None);

        Assert.That(client.BaseAddress?.ToString(), Is.EqualTo(uri));
    }

    // The regression this guards: without the trailing slash, Uri resolution drops "gateway"
    // and the provider silently dials the wrong host path.
    [Test]
    public void CreateClientAsync_ResolvesRelativeProviderPathsUnderTheFullBasePath()
    {
        var client = _factory.CreateClientAsync(CredentialWith("https://gps.example.com/gateway"), CancellationToken.None);

        var resolved = new Uri(client.BaseAddress!, "api/server");

        Assert.That(resolved.ToString(), Is.EqualTo("https://gps.example.com/gateway/api/server"));
    }

    [Test]
    public void CreateClientAsync_WithoutAUri_Throws()
        => Assert.Throws<InvalidOperationException>(
            () => _factory.CreateClientAsync(CredentialWith(string.Empty), CancellationToken.None));
}
