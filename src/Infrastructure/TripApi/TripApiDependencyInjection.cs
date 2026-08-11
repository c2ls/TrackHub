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

using Common.Domain.Constants;
using TrackHub.Router.Domain.Interfaces.Trip;
using TrackHub.Router.Infrastructure.TripApi;

namespace Microsoft.Extensions.DependencyInjection;

// Named TripApiDependencyInjection (not DependencyInjection) because GeofenceApi and ManagerApi
// already own that type name in this namespace — TelemetryApi avoids the clash the same way.
public static class TripApiDependencyInjection
{
    public static IServiceCollection AddAppTripManagementContext(this IServiceCollection services, bool headerPropagation = true)
    {
        // A named client MUST be registered: an unregistered name silently yields a default
        // HttpClient (100 s timeout, no propagation). processTripPositions is a mutation — no retry.
        // headerPropagation must be false outside an HTTP pipeline (SyncWorker) — the
        // propagation handler throws when no request context ever initialized the headers.
        services.AddGraphQLClient(Clients.TripManagement, propagateHeaders: headerPropagation);
        services.AddScoped<ITripPositionWriter, TripPositionWriter>();
        return services;
    }
}
