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

using TrackHub.TripManagement.Domain.Interfaces;
using TrackHub.TripManagement.Infrastructure.TelemetryApi;

namespace Microsoft.Extensions.DependencyInjection;

// Named TelemetryApiDependencyInjection, not DependencyInjection: ManagerApi already ships a
// DependencyInjection class in this same namespace and the two would collide in the host.
public static class TelemetryApiDependencyInjection
{
    public static IServiceCollection AddTelemetryApiContext(this IServiceCollection services)
    {
        // Replay runs under trip_client (Telemetry's positionHistoryRange accepts ServiceClient);
        // this client is query-only, so retries are safe.
        services.AddGraphQLServiceClient(Clients.Telemetry, GraphQLClientResilience.WithRetry);

        services.AddScoped<IPositionHistoryClient, PositionHistoryClient>();

        return services;
    }
}
