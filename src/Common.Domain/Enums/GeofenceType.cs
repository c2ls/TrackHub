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

namespace Common.Domain.Enums;

/// <summary>
/// Category of a geofence. Mirrors the portal list in <c>TrackHub/src/data/geofenceTypes.ts</c>,
/// which stays the label/translation source; the two must be widened together.
/// </summary>
public enum GeofenceType : short
{
    ClientLocation = 1,
    ConstructionSite = 2,
    DangerZone = 3,
    FuelStation = 4,
    Garage = 5,
    Hospital = 6,
    Hotel = 7,
    Office = 8,
    Park = 9,
    ParkingLot = 10,
    RestrictedArea = 11,
    RetailStore = 12,
    School = 13,
    Warehouse = 14,
}
