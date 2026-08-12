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
using TrackHub.TripManagement.Infrastructure.ManagerApi;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddManagerApiContext(this IServiceCollection services)
    {
        // Every Manager call from this module runs under the service's own trip_client identity
        // (client credentials), never the caller's token: the ETA/reminder jobs and the anonymous
        // public-tracking endpoint have no incoming principal at all. NoRetry is the default here
        // because this client carries mutations (alerts, job runs, public-link grants).
        services.AddGraphQLServiceClient(Clients.Manager);

        services.AddScoped<IAlertEmitter, AlertEmitter>();
        services.AddScoped<IBackgroundJobRunRecorder, BackgroundJobRunRecorder>();
        services.AddScoped<IManagerValidationClient, ManagerValidationClient>();
        services.AddScoped<IPublicLinkGrantClient, PublicLinkGrantClient>();
        services.AddScoped<IDocumentClient, DocumentClient>();

        return services;
    }
}
