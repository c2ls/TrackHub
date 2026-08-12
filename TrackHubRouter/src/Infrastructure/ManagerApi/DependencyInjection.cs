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

using ManagerApi;
using TrackHub.Router.Infrastructure.ManagerApi;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddAppManagerContext(this IServiceCollection services, bool headerPropagation = true)
    {
        // Mixed reader/writer surface — retries stay off (GraphQL mutations are POSTs and
        // cannot be distinguished by HTTP method).
        services.AddGraphQLClient(Clients.Manager, propagateHeaders: headerPropagation);

        // Dedicated client for system (client-credentials) calls: never propagates the user's
        // Authorization header, so the factory applies the Router's own service identity.
        services.AddGraphQLServiceClient(Clients.Manager);

        // Master-data / provider-support readers and writers stay on Manager. The positions/history/
        // health/sync-run surface moved to TelemetryApi.
        services.AddScoped<IAccountReader, AccountReader>();

        services.AddMemoryCache();
        services.AddScoped<Common.Application.Interfaces.IAccountOperationalStatusReader, AccountOperationalStatusReader>();
        services.AddScoped<Common.Application.Interfaces.IAccountOperationalStatusService, Common.Application.Services.CachedAccountOperationalStatusService>();

        services.AddScoped<ICredentialWriter, CredentialWriter>();
        services.AddScoped<IGeocodingProviderReader, GeocodingProviderReader>();
        services.AddScoped<IGroupVisibilityReader, GroupVisibilityReader>();
        services.AddScoped<IDeviceTransporterReader, DeviceTransporterReader>();
        services.AddScoped<IOperatorReader, OperatorReader>();
        services.AddScoped<IOperatorSystemReader, OperatorSystemReader>();
        services.AddScoped<ITransporterTypeReader, TransporterTypeReader>();
        services.AddScoped<IDeviceSyncWriter, DeviceSyncWriter>();
        services.AddScoped<IAlertEventWriter, AlertEventWriter>();

        return services;
    }
}
