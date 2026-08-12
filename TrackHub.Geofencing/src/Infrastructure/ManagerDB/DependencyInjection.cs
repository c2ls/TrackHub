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

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TrackHub.Geofencing.Infrastructure.ManagerDB;
using TrackHub.Geofencing.Infrastructure.Readers;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString, o => 
            { 
                o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                o.UseNetTopologySuite();
            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddTrackHubHeaderPropagation();

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IGeofenceWriter, GeofenceWriter>();
        services.AddScoped<IGeofenceReader, GeofenceReader>();
        services.AddScoped<IGeofenceEventReader, GeofenceEventReader>();
        services.AddScoped<IGeofenceEventWriter, GeofenceEventWriter>();
        services.AddScoped<IUserReader, UserReader>();
        services.AddScoped<ITransportersInGeofence, TransportersInGeofence>();
        services.AddScoped<IAccountFeatureReader, AccountFeatureReader>();

        // Cross-service account-status enforcement.
        services.AddMemoryCache();
        services.AddScoped<Common.Application.Interfaces.IAccountOperationalStatusReader, AccountOperationalStatusReader>();
        services.AddScoped<Common.Application.Interfaces.IAccountOperationalStatusService, Common.Application.Services.CachedAccountOperationalStatusService>();

        return services;
    }
}

