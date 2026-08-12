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
using GraphQL.Client.Abstractions;
using HotChocolate.Execution;

namespace TrackHub.ServiceContracts.Harness;

/// <summary>
/// An <see cref="IGraphQLClientFactory"/> whose named clients (e.g. <c>Clients.Manager</c>,
/// <c>Clients.Telemetry</c>) dispatch to in-process producer executors. Consumer classes are
/// constructed against this factory unchanged — the network is the only thing replaced.
/// </summary>
public sealed class InProcessGraphQLClientFactory(IReadOnlyDictionary<string, IRequestExecutor> executorsByClientName) : IGraphQLClientFactory
{
    public IGraphQLClient CreateClient(string name)
        => executorsByClientName.TryGetValue(name, out var executor)
            ? new InProcessGraphQLClient(executor)
            : throw new InvalidOperationException($"No in-process producer registered for client '{name}'.");

    // The service-identity flag only changes which credentials the HTTP client would carry;
    // in-process there is no transport, so both identities hit the same executor.
    public IGraphQLClient CreateClient(string name, bool asService) => CreateClient(name);
}
