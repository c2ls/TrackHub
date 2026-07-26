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

namespace TrackHub.Reporting.Domain.Models;

// Intermediate projections deserialized from TripManagement GraphQL for the spec 11 §13 reports,
// plus the page envelopes the paged feeds return. Names are denormalized producer-side (transporter,
// driver, customer) because a report row is read by a human, not joined by a client.
//
// The trip-level feed is TripVm-shaped with the four route-plan/toll figures and the two display
// names folded in; the stop, toll and POD feeds are purpose-built because TripVm carries no stop,
// station or POD detail at all. See TripReportReader for the exact query documents.

public readonly record struct ReportTripVm(
    Guid TripId, string Code, string Status,
    Guid TransporterId, string TransporterName,
    Guid? DriverId, string? DriverName,
    string? CustomerName,
    DateTimeOffset PlannedStartAt, DateTimeOffset? PlannedEndAt,
    DateTimeOffset? ActualStartAt, DateTimeOffset? ActualEndAt,
    double? PlannedDistanceMeters, double ActualDistanceMeters,
    int StopCount,
    decimal? EstimatedTollAmount, string? TollCurrency, string TollStatus);

public readonly record struct ReportTripStopVm(
    Guid TripStopId, Guid TripId, string TripCode,
    string TransporterName, string? DriverName, string? CustomerName,
    int Sequence, string Name, string Status,
    DateTimeOffset? PlannedArrivalFrom, DateTimeOffset? PlannedArrivalTo,
    DateTimeOffset? ActualArrivalAt, DateTimeOffset? ActualDepartureAt,
    int DeliveryCount, int DeliveredCount, int FailedDeliveryCount, int PartialDeliveryCount);

// One route-plan/station pair. `Amount` is null — never zero — when no tariff covers the trip's
// vehicle class on the plan date; the row surfaces that as PartialNoTariff (spec 11 §7.7).
public readonly record struct ReportTripTollVm(
    Guid TripId, string TripCode, Guid? RoutePlanId, DateTimeOffset PlannedStartAt,
    string? TollVehicleClass,
    Guid TollStationId, string StationName, string? StationCode,
    string? RoadName, string? Direction,
    decimal? Amount, string? Currency, bool HasTariff);

public readonly record struct ReportTripPodVm(
    Guid ProofOfDeliveryId, Guid TripId, string TripCode,
    Guid TripStopId, int StopSequence, string StopName,
    string ReceiverName, string? ReceiverDocument,
    DateTimeOffset CapturedAt, double? Latitude, double? Longitude,
    int DocumentCount);

// ---- Page envelopes ----
//
// NextSkip/HasMore are the producer's cursor and MUST be what the drain loop follows. TotalCount
// is kept for reporting only: the toll feed counts and pages ROUTE PLANS while returning one row
// per matched station, so comparing a collected row count against it ended the drain after one
// page and silently truncated trip-toll-cost.

public readonly record struct TripReportDataPageVm(IEnumerable<ReportTripVm> Items, int TotalCount, int NextSkip, bool HasMore);

public readonly record struct TripStopReportDataPageVm(IEnumerable<ReportTripStopVm> Items, int TotalCount, int NextSkip, bool HasMore);

public readonly record struct TripTollReportDataPageVm(IEnumerable<ReportTripTollVm> Items, int TotalCount, int NextSkip, bool HasMore);

public readonly record struct TripPodReportDataPageVm(IEnumerable<ReportTripPodVm> Items, int TotalCount, int NextSkip, bool HasMore);
