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

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrackHub.TripManagement.Web.Endpoints;

namespace Web.UnitTests;

// Spec 11 §16, second guard: "A DI container-validation test (ValidateOnBuild/ValidateScopes over
// the real Web registrations)."
//
// This exists because of a real production outage. Program.cs was missing
// AddInfrastructureServices, so IGraphQLClientFactory was never registered and every Manager and
// Telemetry client was unresolvable. The service still STARTED and still answered /health — the
// failure only appeared on the first request that touched AlertEmitter, and surfaced as an opaque
// "Unexpected Execution Error".
//
// Nothing else in the suite could have caught it: the application tests mock these interfaces, and
// the contract tests build the GraphQL schema over Mock.Of<ISender>(). This is the only place the
// real container is ever constructed.
[TestFixture]
public sealed class ContainerValidationTests
{
    // Anchored on PublicTrips rather than Program: Program is generated from top-level statements
    // and making it addressable would mean editing production code purely to satisfy a test.
    private sealed class TripManagementFactory : WebApplicationFactory<PublicTrips>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // ValidateOnBuild walks every registered descriptor and fails if any constructor
            // dependency is unregistered — this is the assertion that would have caught the
            // missing IGraphQLClientFactory. ValidateScopes catches a singleton capturing a scoped
            // service, which in this module would silently share a request-scoped DbContext
            // between the two hosted jobs.
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
        using var factory = new TripManagementFactory();

        // Touching Services forces host construction, which is where ValidateOnBuild runs.
        Assert.DoesNotThrow(() => _ = factory.Services);
    }

    // The specific dependencies whose absence is invisible until a request arrives. ValidateOnBuild
    // already covers them transitively, but naming them makes the failure message point at the
    // registration that went missing instead of at a descriptor index.
    [TestCase(typeof(Common.Application.Interfaces.IGraphQLClientFactory))]
    [TestCase(typeof(Common.Application.Interfaces.IFeatureFlagService))]
    [TestCase(typeof(Common.Application.Interfaces.IAccountOperationalStatusReader))]
    public void CriticalCrossServiceDependencies_AreResolvable(Type contract)
    {
        using var factory = new TripManagementFactory();
        using var scope = factory.Services.CreateScope();

        Assert.DoesNotThrow(() => scope.ServiceProvider.GetRequiredService(contract));
    }
}
