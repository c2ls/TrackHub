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
using TrackHub.Router.Domain.Interfaces;

namespace TrackHub.Router.Infrastructure.RedGps;

public sealed class ProviderDescriptor : IProviderDescriptor
{
    public ProtocolType Protocol => ProtocolType.RedGps;

    public string DisplayName => "RedGPS";

    public ProviderCapability Capabilities =>
        ProviderCapability.RealTimePositions
        | ProviderCapability.PositionHistory
        | ProviderCapability.DeviceCatalog
        | ProviderCapability.ConnectivityPing;

    // RedGPS requires 30 seconds between any two requests; 60 leaves a slot for device sync and health.
    public int MinPositionIntervalSeconds => 60;

    public ProviderCredentialField CredentialFields =>
        ProviderCredentialField.Username
        | ProviderCredentialField.Password
        | ProviderCredentialField.Key;

    public string? DefaultUri => "https://api.service24gps.com/api/v1";
}
