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

namespace Common.Web.Infrastructure;

// Extension methods for the WebApplication class.
public static class WebApplicationExtensions
{
    /// <summary>
    /// Maps an endpoint group to the web application.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    /// <param name="group">The EndpointGroupBase instance representing the endpoint group to be mapped.</param>
    /// <returns>A RouteGroupBuilder instance for further configuration.</returns>
    public static RouteGroupBuilder MapGroup(this WebApplication app, EndpointGroupBase group)
    {
        var groupName = group.GetType().Name;

        return app.MapGroup($"/api/{groupName}")
            .WithTags(groupName);
    }

    /// <summary>
    /// Maps all endpoint groups in the specified assembly to the web application.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    /// <param name="assembly">The Assembly containing the endpoint group types.</param>
    /// <returns>The modified WebApplication instance.</returns>
    public static WebApplication MapEndpoints(this WebApplication app, Assembly assembly)
    {
        var endpointGroupType = typeof(EndpointGroupBase);

        var endpointGroupTypes = assembly.GetExportedTypes()
            .Where(t => t.IsSubclassOf(endpointGroupType));

        foreach (var type in endpointGroupTypes)
        {
            if (Activator.CreateInstance(type) is EndpointGroupBase instance)
            {
                instance.Map(app);
            }
        }

        return app;
    }
}
