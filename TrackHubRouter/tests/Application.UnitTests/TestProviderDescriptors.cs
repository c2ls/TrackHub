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
using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Interfaces;

namespace Application.UnitTests;

/// <summary>
/// Descriptor stand-ins for handler tests. The real declarations live in the provider
/// assemblies; here the catalog is built from these so Application tests stay independent
/// of the provider projects. GpsGate keeps its no-history shape because the capability
/// short-circuit is part of the handler contract under test.
/// </summary>
public static class TestProviderDescriptors
{
    private const ProviderCapability Full =
        ProviderCapability.RealTimePositions
        | ProviderCapability.PositionHistory
        | ProviderCapability.DeviceCatalog
        | ProviderCapability.ConnectivityPing;

    public sealed record TestDescriptor(ProtocolType Protocol, string DisplayName, ProviderCapability Capabilities) : IProviderDescriptor;

    public static ProviderCapabilityCatalog DefaultCatalog { get; } = new(
    [
        new TestDescriptor(ProtocolType.CommandTrack, "CommandTrack", Full),
        new TestDescriptor(ProtocolType.Traccar, "Traccar", Full),
        new TestDescriptor(ProtocolType.GpsGate, "GpsGate", Full & ~ProviderCapability.PositionHistory),
    ]);
}
