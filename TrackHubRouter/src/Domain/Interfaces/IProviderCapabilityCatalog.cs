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
/// The runtime truth about what each registered GPS provider's API supports, aggregated
/// from the provider assemblies' own <see cref="IProviderDescriptor"/> declarations at
/// startup. A capability listed here means the provider implements it against its external
/// API; a missing capability is a PROVIDER limitation, surfaced to clients as
/// <c>ProviderCapabilityNotSupportedException</c> (GraphQL code
/// PROVIDER_CAPABILITY_NOT_SUPPORTED) — never confused with TrackHub feature gating
/// (FEATURE_DISABLED) or a missing provider assembly (ProtocolNotSupportedException).
/// </summary>
public interface IProviderCapabilityCatalog
{
    ProviderCapability Get(ProtocolType protocol);

    bool Supports(ProtocolType protocol, ProviderCapability capability);

    /// <summary>Throws the client-facing provider-limitation error when the capability is missing.</summary>
    void EnsureSupports(ProtocolType protocol, ProviderCapability capability);

    /// <summary>Every registered provider's declaration, for the capability-matrix query.</summary>
    IReadOnlyCollection<IProviderDescriptor> All { get; }
}
