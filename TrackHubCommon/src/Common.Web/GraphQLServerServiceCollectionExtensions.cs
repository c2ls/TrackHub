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

using Common.Web.Infrastructure;
using HotChocolate.Execution.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class GraphQLServerServiceCollectionExtensions
{
    /// <summary>
    /// Registers a GraphQL server with the shared TrackHub hardening: authorization,
    /// max execution depth, the common error filter, and exception-detail exposure.
    /// This is the single source of truth for the server configuration every service uses;
    /// per-service deviations (extra error filters, etc.) are applied on the returned builder.
    /// </summary>
    /// <typeparam name="TQuery">The service's root query type.</typeparam>
    /// <typeparam name="TMutation">The service's root mutation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="includeExceptionDetails">True to include exception details in errors (development only).</param>
    /// <returns>The request executor builder, for per-service additions.</returns>
    public static IRequestExecutorBuilder AddTrackHubGraphQLServer<TQuery, TMutation>(
        this IServiceCollection services, bool includeExceptionDetails)
        where TQuery : class
        where TMutation : class
        => services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddMaxExecutionDepthRule(15)
            .AddErrorFilter<TrackHubGraphQLErrorFilter>()
            .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = includeExceptionDetails)
            .AddQueryType<TQuery>()
            .AddMutationType<TMutation>();
}
