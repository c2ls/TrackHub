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

/// <summary>
/// Backing entity for trip.vw_visible_transporter - the SINGLE source of portal group visibility
/// for this service (spec 11 section 5, acceptance 4). No handler re-implements the group graph;
/// the view joins app.user_group, app.transporter_group and app.transporters so visibility costs
/// no cross-service call.
/// </summary>
public sealed class VwVisibleTransporter
{
    public Guid UserId { get; set; }
    public Guid TransporterId { get; set; }
    public Guid AccountId { get; set; }
}
