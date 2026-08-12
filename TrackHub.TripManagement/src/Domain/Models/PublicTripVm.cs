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

namespace TrackHub.TripManagement.Domain.Models;

/// <summary>
/// The anonymous tracking snapshot. This type is the disclosure boundary: it is projected from
/// the <c>TripShare</c> field flags server-side, never filtered client-side. It deliberately
/// carries no ids beyond the trip's, no internal notes, no toll or cost figures, no driver
/// contact data, no raw position history and no document bytes (spec 11 §7.8, acceptance 23).
/// <para>
/// <b>There is deliberately no <c>CustomerName</c>.</b> On a multi-drop trip the consignee is not
/// the same party as every link holder, and no <c>TripShare</c> flag gates it — so carrying it here
/// disclosed the shipper's customer unconditionally to every recipient of every link on that trip
/// (spec 11 §7.8). Do not re-add it; a per-recipient consignee would have to be scoped to the
/// share, not to the trip.
/// </para>
/// </summary>
public readonly record struct PublicTripVm(
    Guid TripId,
    string Code,
    string Status,
    DateTimeOffset PlannedStartAt,
    DateTimeOffset? PlannedEndAt,
    DateTimeOffset? ActualStartAt,
    DateTimeOffset? ActualEndAt,
    IReadOnlyCollection<PublicTripStopVm> Stops,
    string? VehicleLabel,
    string? DriverGivenName,
    double? LastLatitude,
    double? LastLongitude,
    DateTimeOffset? LastPositionAt,
    GeometryLineVm? PlannedRoute);

/// <summary>
/// A stop as an end customer sees it. <paramref name="HasProofOfDelivery"/> is an existence
/// boolean only — POD content and documents are never exposed by a public link.
/// </summary>
public readonly record struct PublicTripStopVm(
    int Sequence,
    string Name,
    string? City,
    string Status,
    DateTimeOffset? PlannedArrivalFrom,
    DateTimeOffset? PlannedArrivalTo,
    DateTimeOffset? ActualArrivalAt,
    DateTimeOffset? EtaAt,
    bool HasProofOfDelivery);

/// <summary>
/// Outcome of resolving a public link, mapped straight onto the HTTP status the endpoint returns.
/// A discriminated result rather than an exception, because 404 and 410 are both normal answers.
/// </summary>
public enum PublicTripResolution
{
    /// <summary>Resolved — project the configured snapshot.</summary>
    Found = 0,

    /// <summary>No such grant, revoked, wrong scope, or the account lost <c>trip-management</c>.</summary>
    NotFound = 1,

    /// <summary>The grant existed and was valid, but its expiry has passed.</summary>
    Expired = 2,
}

public readonly record struct PublicTripResultVm(PublicTripResolution Resolution, PublicTripVm? Trip);
