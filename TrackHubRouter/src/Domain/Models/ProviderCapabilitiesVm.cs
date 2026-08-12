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

namespace TrackHub.Router.Domain.Models;

// One row of the provider capability matrix (IProviderCapabilityCatalog) as exposed to clients.
// Flat booleans rather than the [Flags] enum so the GraphQL shape stays additive when a
// capability is introduced. DisplayName is the provider's client-facing name, declared by the
// provider assembly itself, so clients need no local protocol-name table.
public record struct ProviderCapabilitiesVm(
    int ProtocolTypeId,
    string Protocol,
    string DisplayName,
    bool RealTimePositions,
    bool PositionHistory,
    bool DeviceCatalog,
    bool ConnectivityPing);
