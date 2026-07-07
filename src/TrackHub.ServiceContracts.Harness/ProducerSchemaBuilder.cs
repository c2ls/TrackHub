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

using Common.Mediator;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TrackHub.ServiceContracts.Harness;

/// <summary>
/// Builds a producer's REAL schema and executor in process, using the same
/// <c>AddTrackHubGraphQLServer</c> extension every service's Program.cs calls,
/// so the tests guard the configuration production ships — not a copy. Only the data access
/// is replaced: every resolver dispatches through <see cref="ISender"/>, which the caller fakes.
/// Producer-specific deviations (extra error filters) are applied via <c>configure</c>.
/// </summary>
public static class ProducerSchemaBuilder
{
    public static async Task<ISchemaDefinition> BuildSchemaAsync<TQuery, TMutation>(
        ISender sender, Action<IRequestExecutorBuilder>? configure = null)
        where TQuery : class
        where TMutation : class
        => await CreateBuilder<TQuery, TMutation>(sender, configure).BuildSchemaAsync();

    public static async Task<IRequestExecutor> BuildExecutorAsync<TQuery, TMutation>(
        ISender sender, Action<IRequestExecutorBuilder>? configure = null)
        where TQuery : class
        where TMutation : class
        => await CreateBuilder<TQuery, TMutation>(sender, configure).BuildRequestExecutorAsync();

    private static IRequestExecutorBuilder CreateBuilder<TQuery, TMutation>(
        ISender sender, Action<IRequestExecutorBuilder>? configure)
        where TQuery : class
        where TMutation : class
    {
        var services = new ServiceCollection();

        // Execution resolves the ASP.NET authorization service (the schema carries
        // AddAuthorization), which in turn needs logging; no field is [Authorize]-gated.
        services.AddLogging();
        services.AddSingleton(sender);

        var builder = services.AddTrackHubGraphQLServer<TQuery, TMutation>(includeExceptionDetails: true);
        configure?.Invoke(builder);
        return builder;
    }
}
