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

namespace TrackHub.TripManagement.Infrastructure.TripDB.Entities;

// Read-only projection of the Manager-owned app.drivers table. Name only: the report feeds need a
// display label, and a driver's phone or document number is deliberately NOT mapped here so it
// cannot leak into an export or a public snapshot (spec 11 section 4, acceptance 23).
public sealed class Driver
{
    public Guid DriverId { get; set; }
    public Guid AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
}
