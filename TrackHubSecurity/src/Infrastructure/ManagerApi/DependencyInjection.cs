// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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
using TrackHub.Security.Domain.Interfaces;
using TrackHub.Security.Infrastructure.ManagerApi;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
    public static IServiceCollection AddAppManagerContext(this IServiceCollection services)
    {
        // User-mirror writer carries mutations — no retry.
        services.AddGraphQLClient(Clients.Manager);

        // As-service client for security audit forwarding: authenticates with the security_client
        // credentials (no user-token propagation). The factory attaches the bearer token itself.
        services.AddGraphQLServiceClient(Clients.Manager);

        services.AddScoped<IManagerWriter, ManagerWriter>();
        services.AddScoped<IManagerAuditWriter, ManagerAuditWriter>();

        return services;
    }
}
