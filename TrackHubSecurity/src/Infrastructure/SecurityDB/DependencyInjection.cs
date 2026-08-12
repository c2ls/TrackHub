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

using Common.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using TrackHub.Security.Infrastructure.SecurityDB;
using TrackHub.Security.Infrastructure.SecurityDB.Identity;
using TrackHub.Security.Infrastructure.SecurityDB.Interfaces;
using TrackHub.Security.Infrastructure.SecurityDB.Readers;
using TrackHub.Security.Infrastructure.SecurityDB.Writers;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Security");

        Guard.Against.Null(connectionString, message: "Connection string 'Security' not found.");

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        //This is required to call user defined methods in the Management api
        services.AddSingleton<IGraphQLClientFactory, Common.Infrastructure.GraphQLClientFactory>();
        //This is required to use the local Identity implementation
        services.AddScoped<IIdentityService, IdentityService>();

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IActionReader, ActionReader>();
        services.AddScoped<IClientReader, ClientReader>();
        services.AddScoped<IClientWriter, ClientWriter>();
        services.AddScoped<IDriverIdentityReader, DriverIdentityReader>();
        services.AddScoped<IDriverIdentityWriter, DriverIdentityWriter>();
        services.AddScoped<IPolicyReader, PolicyReader>();
        services.AddScoped<IResourceReader, ResourceReader>();
        services.AddScoped<IResourceActionPolicyWriter, ResourceActionPolicyWriter>();
        services.AddScoped<IResourceActionPolicyReader, ResourceActionPolicyReader>();
        services.AddScoped<IResourceActionRoleWriter, ResourceActionRoleWriter>();
        services.AddScoped<IResourceActionRoleReader, ResourceActionRoleReader>();
        services.AddScoped<IRoleReader, RoleReader>();
        services.AddScoped<IServiceClientPermissionReader, ServiceClientPermissionReader>();
        services.AddScoped<IServiceClientPermissionWriter, ServiceClientPermissionWriter>();
        services.AddScoped<IUserPolicyWriter, UserPolicyWriter>();
        services.AddScoped<IUserPolicyReader, UserPolicyReader>();
        services.AddScoped<IUserWriter, UserWriter>();
        services.AddScoped<IUserReader, UserReader>();
        services.AddScoped<IUserRoleWriter, UserRoleWriter>();
        services.AddScoped<IUserRoleReader, UserRoleReader>();

        return services;
    }
}
