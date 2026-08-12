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
using Common.Application;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddApplicationServices(assembly);
        services.AddDistributedMemoryCache();
        services.AddMemoryCache();
        // Single request-side mode resolver: all position handlers resolve the
        // gps.integration / gps.positionHistory flags through here, with a 60-second cache.
        services.AddScoped<TrackHub.Router.Application.Gating.IAccountModeResolver, TrackHub.Router.Application.Gating.AccountModeResolver>();
        return services;
    }
}
