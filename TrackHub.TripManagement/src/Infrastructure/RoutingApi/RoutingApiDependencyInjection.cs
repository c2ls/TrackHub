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

using Microsoft.Extensions.Configuration;
using TrackHub.TripManagement.Domain.Interfaces;
using TrackHub.TripManagement.Infrastructure.RoutingApi;

namespace Microsoft.Extensions.DependencyInjection;

// Named RoutingApiDependencyInjection, not DependencyInjection: ManagerApi already ships a
// DependencyInjection class in this same namespace and the two would collide in the host.
public static class RoutingApiDependencyInjection
{
    private const int DefaultTimeoutSeconds = 30;

    public static IServiceCollection AddRoutingApiContext(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(RoutingOptions.SectionName);
        services.Configure<RoutingOptions>(section);

        // The timeout is a client-construction concern, so it is read eagerly rather than
        // through IOptions; everything else the provider reads at call time.
        var timeoutSeconds = int.TryParse(section["TimeoutSeconds"], out var configured) && configured > 0
            ? configured
            : DefaultTimeoutSeconds;

        services.AddHttpClient(
            OpenRouteServiceRoutingProvider.HttpClientName,
            client => client.Timeout = TimeSpan.FromSeconds(timeoutSeconds));

        services.AddScoped<IRoutingProvider, OpenRouteServiceRoutingProvider>();

        return services;
    }
}
