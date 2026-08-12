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
using TrackHub.Security.Web.GraphQL.Query;

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
    private sealed class SecurityFactory : WebApplicationFactory<Query>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // ValidateOnBuild walks every registered descriptor and fails if any constructor
            // dependency is unregistered. ValidateScopes catches a singleton capturing a scoped
            // service — here that would leak one request's DbContext into every later request.
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
        using var factory = new SecurityFactory();

        // Touching Services forces host construction, which is where ValidateOnBuild runs.
        Assert.DoesNotThrow(() => _ = factory.Services);
    }

    // The dependencies whose absence is invisible until a request arrives. ValidateOnBuild already
    // covers them transitively; naming them makes the failure point at the missing registration
    // rather than at a descriptor index.
    [TestCase(typeof(Common.Application.Interfaces.IGraphQLClientFactory))]
    public void CriticalCrossServiceDependencies_AreResolvable(Type contract)
    {
        using var factory = new SecurityFactory();
        using var scope = factory.Services.CreateScope();

        Assert.DoesNotThrow(() => scope.ServiceProvider.GetRequiredService(contract));
    }
}
