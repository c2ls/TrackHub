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
using Common.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application.Extensions;

public static class ModuleDiscoveryExtensions
{
    /// <summary>
    /// Discovers every <see cref="IServiceModule"/> implementation in <paramref name="assembly"/>
    /// and invokes its registrations. Modules run in deterministic (full-name) order.
    /// </summary>
    public static IServiceCollection AddDiscoveredModules(
        this IServiceCollection services,
        Assembly assembly,
        IConfiguration configuration)
    {
        var modules = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && typeof(IServiceModule).IsAssignableFrom(t))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .Select(t => (IServiceModule)Activator.CreateInstance(t)!);

        foreach (var module in modules)
        {
            module.Register(services, configuration);
        }

        return services;
    }
}
