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
using TrackHub.Router.Domain.Exceptions;
using TrackHub.Router.Domain.Interfaces;

namespace TrackHub.Router.Domain.Helpers;

/// <summary>
/// <see cref="IProviderCapabilityCatalog"/> built once at startup from the discovered
/// provider descriptors (see <c>ProtocolRegistrationExtensions</c>). Unknown protocols
/// report <see cref="ProviderCapability.None"/>, so every capability check on them fails
/// as a provider limitation rather than a masked server error.
/// </summary>
public sealed class ProviderCapabilityCatalog(IEnumerable<IProviderDescriptor> descriptors) : IProviderCapabilityCatalog
{
    private readonly IReadOnlyDictionary<ProtocolType, IProviderDescriptor> _catalog =
        descriptors.ToDictionary(d => d.Protocol);

    public ProviderCapability Get(ProtocolType protocol)
        => _catalog.TryGetValue(protocol, out var descriptor) ? descriptor.Capabilities : ProviderCapability.None;

    public bool Supports(ProtocolType protocol, ProviderCapability capability)
        => (Get(protocol) & capability) == capability;

    public void EnsureSupports(ProtocolType protocol, ProviderCapability capability)
    {
        if (!Supports(protocol, capability))
        {
            throw new ProviderCapabilityNotSupportedException(protocol, capability);
        }
    }

    public IReadOnlyCollection<IProviderDescriptor> All => [.. _catalog.Values];
}
