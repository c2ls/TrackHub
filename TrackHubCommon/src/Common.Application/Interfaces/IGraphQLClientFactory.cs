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

namespace Common.Application.Interfaces;
public interface IGraphQLClientFactory
{
    IGraphQLClient CreateClient(string name);

    /// <summary>
    /// Creates a client for the named GraphQL service. When <paramref name="asService"/> is true the
    /// client authenticates with the host's client-credentials (service) identity even if the host
    /// normally propagates user tokens; use it for system operations that must not depend on the
    /// calling user's permissions.
    /// </summary>
    IGraphQLClient CreateClient(string name, bool asService);
}
