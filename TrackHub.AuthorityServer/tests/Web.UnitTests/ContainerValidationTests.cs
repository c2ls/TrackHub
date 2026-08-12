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

using Microsoft.AspNetCore.Mvc.Testing;
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
    // Anchored on a public Web type rather than Program: Program is generated from top-level
    // statements and making it addressable would mean editing production code to satisfy a test.
    private sealed class AuthorityServerFactory : WebApplicationFactory<LoginController>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
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
