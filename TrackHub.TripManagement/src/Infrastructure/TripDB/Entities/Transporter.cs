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

// Read-only projection of the Manager-owned app.transporters table. Only the type id is needed,
// and only so the account-scoped toll-class mapping can resolve a transporter to its vehicle class
// without a per-request cross-service call - the same rationale as the visibility view.
public sealed class Transporter
{
    public Guid TransporterId { get; set; }
    public Guid AccountId { get; set; }
    public short TransporterTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
}
