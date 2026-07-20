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

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TrackHub.Telemetry.Infrastructure.TelemetryDB;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        // Dynamic JSON is required for the transporter_position `attributes` json column (AttributesVm).
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.ConfigureJsonOptions(new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(dataSource, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddTrackHubHeaderPropagation();

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IVisibleTransporterReader, VisibleTransporterReader>();
        services.AddScoped<ITransporterPositionReader, TransporterPositionReader>();
        services.AddScoped<ITransporterPositionWriter, TransporterPositionWriter>();
        services.AddScoped<ITransporterPositionHistoryReader, TransporterPositionHistoryReader>();
        services.AddScoped<ITransporterPositionHistoryWriter, TransporterPositionHistoryWriter>();
        services.AddScoped<IOperatorSyncRunReader, OperatorSyncRunReader>();
        services.AddScoped<IOperatorSyncRunWriter, OperatorSyncRunWriter>();
        services.AddScoped<IOperatorHealthCheckReader, OperatorHealthCheckReader>();
        services.AddScoped<IOperatorHealthCheckWriter, OperatorHealthCheckWriter>();
        services.AddScoped<IPositionRetentionPolicyReader, PositionRetentionPolicyReader>();
        services.AddScoped<IPlatformSyncActivityReader, PlatformSyncActivityReader>();
        services.AddScoped<IResolvedAddressWriter, ResolvedAddressWriter>();

        // Cross-service account-status enforcement.
        services.AddMemoryCache();
        services.AddScoped<Common.Application.Interfaces.IAccountOperationalStatusReader, AccountOperationalStatusReader>();
        services.AddScoped<Common.Application.Interfaces.IAccountOperationalStatusService, Common.Application.Services.CachedAccountOperationalStatusService>();

        return services;
    }
}
