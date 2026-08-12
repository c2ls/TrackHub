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

namespace TrackHub.Router.Domain.Enumerators;

// What a GPS provider's API can do, independent of TrackHub features or account billing.
// RealTimePositions and PositionHistory are the two capabilities clients care about;
// DeviceCatalog and ConnectivityPing exist so a future partial provider (one that ships
// without a DeviceReader or ConnectivityTester) stays representable.
[Flags]
public enum ProviderCapability
{
    None = 0,
    RealTimePositions = 1,
    PositionHistory = 2,
    DeviceCatalog = 4,
    ConnectivityPing = 8,
}
