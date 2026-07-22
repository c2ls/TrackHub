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

using Common.Infrastructure;

namespace TrackHub.TripManagement.Infrastructure.TripDB.Entities;

/// <summary>
/// The account-scoped half of toll configuration: which vehicle class a transporter is priced as.
/// Account-scoped precisely because fleet composition IS tenant data, unlike the platform catalog
/// (spec 11 section 5). A row-level <see cref="TransporterId"/> override wins over the type
/// mapping.
/// </summary>
public sealed class TransporterTollClass : BaseAuditableEntity
{
    public Guid TransporterTollClassId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public short? TransporterTypeId { get; set; }
    public Guid? TransporterId { get; set; }
    public string TollVehicleClassCode { get; set; } = string.Empty;
}
