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
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Infrastructure.SecurityApi;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddAppSecurityContext(this IServiceCollection services)
    {
        // User-provisioning writer carries mutations — no retry.
        services.AddGraphQLClient(Clients.Security);
        // Staff offboarding revokes Security credentials under Manager's own identity: the caller's
        // token is not what Security grants that to.
        services.AddGraphQLServiceClient(Clients.Security);
        services.AddScoped<IStaffCredentialRevoker, StaffCredentialRevoker>();

        services.AddScoped<ISecurityWriter, SecurityWriter>();

        return services;
    }
}
