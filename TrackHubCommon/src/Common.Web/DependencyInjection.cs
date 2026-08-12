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

using Common.Application.Interfaces;
using Common.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddScoped<IUser, CurrentUser>();
        services.AddScoped<ICurrentPrincipal>(sp => sp.GetRequiredService<IUser>());
        services.AddHttpContextAccessor();
        services.AddExceptionHandler<CustomExceptionHandler>();

        // Customize default API behaviour
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        services.AddEndpointsApiExplorer();

        return services;
    }

    public static IServiceCollection AddWorkerServices(this IServiceCollection services)
    {
        services.AddScoped<IUser, CurrentUser>();
        services.AddScoped<ICurrentPrincipal>(sp => sp.GetRequiredService<IUser>());
        services.AddHttpContextAccessor();
        services.AddExceptionHandler<CustomExceptionHandler>();

        return services;
    }
}
