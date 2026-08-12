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

using GraphQL.Client.Abstractions;

namespace ManagerApi;

/// <summary>
/// <see cref="IOperatorSystemReader"/> over a client-credentials GraphQL client: the user's
/// Authorization header is never propagated, so Manager sees <c>router_client</c> and returns
/// decrypted credentials.
/// </summary>
/// <remarks>
/// Reuses <see cref="OperatorReader"/>'s query documents so both identities send byte-identical
/// requests and the contract tests cover one surface.
/// </remarks>
public sealed class OperatorSystemReader(IGraphQLClientFactory graphQLClient)
    : OperatorReader(graphQLClient.CreateClient(Clients.Manager, asService: true)), IOperatorSystemReader;
