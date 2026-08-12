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

using Common.Domain.Enums;
using TrackHub.Router.Domain.Enumerators;

namespace TrackHub.Router.Domain.Interfaces;

/// <summary>
/// A provider assembly's self-declaration: which protocol it implements, how the provider
/// is named for clients, and which capabilities its external API supports. Each provider
/// assembly ships exactly one implementation named <c>ProviderDescriptor</c> in its root
/// namespace; the registration path discovers it and builds the runtime
/// <see cref="IProviderCapabilityCatalog"/>, cross-checking the declaration against the
/// reader classes actually present in the assembly (fail fast on mismatch).
/// </summary>
public interface IProviderDescriptor
{
    ProtocolType Protocol { get; }
    string DisplayName { get; }
    ProviderCapability Capabilities { get; }
}
