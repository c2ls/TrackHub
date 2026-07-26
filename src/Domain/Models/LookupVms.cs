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

namespace TrackHub.Manager.Domain.Models;

// Picker projections. A picker needs the whole set — a truncated one silently hides options — so
// these reads are NOT paged; they carry only the columns a select control binds and are capped by
// LookupLimits, which THROWS past the ceiling instead of returning a short list.

/// <summary>
/// Minimal transporter projection for select controls. The type travels with it because the
/// toll-class dialog derives its whole transporter-type list from the picker feed.
/// </summary>
public readonly record struct TransporterLookupVm(
    Guid TransporterId,
    string Name,
    TransporterType TransporterType,
    short TransporterTypeId);

/// <summary>
/// Minimal point-of-interest projection for select controls. Carries what the map overlays draw as
/// well as the identity: coordinates place the pin, colour paints it, and type/description/address
/// fill the popup — so the dashboard and the route planner render straight from this feed instead
/// of draining the paged read. Everything else on a POI stays off it.
/// </summary>
public readonly record struct PointOfInterestLookupVm(
    Guid PointOfInterestId,
    string Name,
    double Latitude,
    double Longitude,
    short? Color,
    short Type,
    string? Description,
    string? Address,
    bool Active);

/// <summary>Minimal group projection for select controls.</summary>
public readonly record struct GroupLookupVm(
    long GroupId,
    string Name);

/// <summary>Minimal operator projection for select controls.</summary>
public readonly record struct OperatorLookupVm(
    Guid OperatorId,
    string Name);

/// <summary>Minimal user projection for select controls.</summary>
public readonly record struct UserLookupVm(
    Guid UserId,
    string Username);

/// <summary>
/// Minimal device projection for select controls. The owning operator travels with it because the
/// dashboard's operator filter joins operator→device→transporter over the whole account.
/// </summary>
public readonly record struct DeviceLookupVm(
    Guid DeviceId,
    string Name,
    Guid OperatorId);
