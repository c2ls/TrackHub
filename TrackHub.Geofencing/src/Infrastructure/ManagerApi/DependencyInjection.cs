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

using TrackHub.Geofencing.Infrastructure.ManagerApi;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddManagerApiContext(this IServiceCollection services)
    {
        // Alert emission and job-run recording always run under the service's own
        // geofence_client identity (client credentials), never the caller's token —
        // the dwell evaluator has no incoming request at all.
        services.AddGraphQLServiceClient(Clients.Manager);

        services.AddScoped<IAlertEmitter, AlertEmitter>();
        services.AddScoped<IBackgroundJobRunRecorder, BackgroundJobRunRecorder>();

        return services;
    }
}
