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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application.Interfaces;

/// <summary>
/// A self-registering feature module. Implementations bundle every registration a feature
/// area needs (readers/writers, hosted services, clients, contexts) so composition roots
/// stay untouched as modules are added: <c>AddDiscoveredModules</c> finds all
/// implementations in an assembly and invokes them. Implementations must have a public
/// parameterless constructor.
/// </summary>
public interface IServiceModule
{
    void Register(IServiceCollection services, IConfiguration configuration);
}
