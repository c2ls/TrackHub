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

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrackHub.AuthorityServer.Web.Controllers;
using TrackHub.AuthorityServer.Web.Endpoints;

namespace Web.UnitTests;

// The TripManagement precedent, applied here (TT-03). That service shipped a Program.cs missing
// AddInfrastructureServices: it STARTED, it answered /health, and the failure only appeared on the
// first request that touched a cross-service client. Nothing else in a suite can catch that — the
// application tests mock these interfaces and the contract tests build the schema over mocks. This
// is the only place the real container is ever constructed.
[TestFixture]
public sealed class ContainerValidationTests
{
    // A Release build takes the file-loading branch of AddOpenIdDictServices, and appsettings.json
    // points OpenIddict:Path at an operator-provisioned path that exists on no build agent. The
    // certificate is a deployment input, so the fixture supplies a throwaway one rather than the
    // suite asserting a machine layout — which also means the file-loading branch itself is
    // exercised instead of the DEBUG development-certificate shortcut.
    private const string CertificatePassword = "container-validation";
    private static string _certificatePath = string.Empty;

    [OneTimeSetUp]
    public void CreateSigningCertificate()
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=TrackHub AuthorityServer Container Validation",
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        using var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1));

        _certificatePath = Path.Combine(Path.GetTempPath(), $"trackhub-authority-{Guid.NewGuid():N}.pfx");
        File.WriteAllBytes(_certificatePath, certificate.Export(X509ContentType.Pkcs12, CertificatePassword));
    }

    [OneTimeTearDown]
    public void DeleteSigningCertificate()
    {
        if (_certificatePath.Length > 0)
        {
            File.Delete(_certificatePath);
        }
    }

    // Anchored on a public Web type rather than Program: Program is generated from top-level
    // statements and making it addressable would mean editing production code to satisfy a test.
    private sealed class AuthorityServerFactory : WebApplicationFactory<LoginController>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Appended after the application's own sources, so it outranks appsettings.json.
            builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["OpenIddict:LoadCertFromFile"] = "true",
                    ["OpenIddict:Path"] = _certificatePath,
                    ["OpenIddict:Password"] = CertificatePassword,
                }));

            // ValidateOnBuild walks every registered descriptor and fails if any constructor
            // dependency is unregistered. ValidateScopes catches a singleton capturing a scoped
            // service — here that would mean an OpenIddict store holding one request's DbContext
            // for the lifetime of the process.
            builder.UseDefaultServiceProvider(options =>
            {
                options.ValidateOnBuild = true;
                options.ValidateScopes = true;
            });

            return base.CreateHost(builder);
        }
    }

    [Test]
    public void RealWebRegistrations_BuildAValidContainer()
    {
        using var factory = new AuthorityServerFactory();

        // Touching Services forces host construction, which is where ValidateOnBuild runs.
        Assert.DoesNotThrow(() => _ = factory.Services);
    }

    // The two handlers the minimal-API endpoints resolve out of the request scope. An unregistered
    // handler here fails at /authorize or /token, not at startup — the whole class of defect this
    // fixture exists for.
    [TestCase(typeof(AuthorizationHandler))]
    [TestCase(typeof(TokenHandler))]
    public void CriticalRequestScopedHandlers_AreResolvable(Type contract)
    {
        using var factory = new AuthorityServerFactory();
        using var scope = factory.Services.CreateScope();

        Assert.DoesNotThrow(() => scope.ServiceProvider.GetRequiredService(contract));
    }
}
