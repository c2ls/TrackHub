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

using TrackHub.Router.Domain.Enumerators;

namespace TrackHub.Router.Domain.Exceptions;

// The operator's GPS provider does not offer the requested capability. Distinct from
// ProtocolNotSupportedException (deployment/config gap: no reader registered at all) and from
// FeatureDisabledException (TrackHub account gating): this one attributes the limitation to the
// external provider so a paying client is never told — or led to believe — that TrackHub is
// withholding the feature. Mapped to GraphQL code PROVIDER_CAPABILITY_NOT_SUPPORTED.
public sealed class ProviderCapabilityNotSupportedException(
    Common.Domain.Enums.ProtocolType protocol,
    ProviderCapability capability)
    : Exception($"The GPS provider '{protocol}' does not support {Describe(capability)}. "
        + "This is a limitation of the provider's API, not a TrackHub restriction.")
{
    public Common.Domain.Enums.ProtocolType Protocol { get; } = protocol;
    public ProviderCapability Capability { get; } = capability;

    private static string Describe(ProviderCapability capability)
        => capability switch
        {
            ProviderCapability.RealTimePositions => "real-time positions",
            ProviderCapability.PositionHistory => "position history",
            ProviderCapability.DeviceCatalog => "device-catalog listing",
            ProviderCapability.ConnectivityPing => "connectivity checks",
            _ => capability.ToString(),
        };
}
