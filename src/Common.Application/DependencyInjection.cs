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

using System.Reflection;
using Ardalis.GuardClauses;
using Common.Application.Behaviors;
using Common.Application.Interfaces;
using Common.Application.Services;
using Common.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Common.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, Assembly? assembly, bool isGraphQL = true)
    {
        services.AddValidatorsFromAssembly(assembly);
        Guard.Against.Null(assembly, message: $"Application assemblies not loaded.");

        services.TryAddScoped<IFeatureFlagService, AlwaysEnabledFeatureFlagService>();
        services.TryAddScoped<IAccountOperationalStatusService, AlwaysOperationalAccountStatusService>();

        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
            // Tenant scope is enforced immediately after authorization: the principal (and its
            // permission grant) is established, and no other behavior or handler has yet acted on
            // the account named by the request.
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AccountScopeBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AccountStatusBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(FeatureFlagBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RateLimitingBehavior<,>));
            if (isGraphQL)
            {
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(GraphQLValidationBehavior<,>));
            }
            else
            {
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            }
        });

        return services;
    }

    public static IServiceCollection AddBasicApplicationServices(this IServiceCollection services, Assembly? assembly)
    {
        services.AddValidatorsFromAssembly(assembly);
        Guard.Against.Null(assembly, message: $"Application assemblies not loaded.");

        services.AddMediator(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
