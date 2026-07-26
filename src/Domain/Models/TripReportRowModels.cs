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

// Row VMs for the six spec 11 §13 trip reports.
// Property order = Excel/PDF column order; property name = resx header key (all three Resources*.resx).

public readonly record struct TripSummaryRowVm(
    string TripCode, string TripStatus, string TransporterName, string DriverName,
    DateTimeOffset PlannedStartAt, DateTimeOffset? ActualStartAt,
    DateTimeOffset? PlannedEndAt, DateTimeOffset? ActualEndAt,
    double? PlannedDistanceKm, double ActualDistanceKm,
    int StopCount, bool? OnTime,
    decimal? EstimatedTollAmount, string TollCurrency);

public readonly record struct TripStopDetailRowVm(
    string TripCode, int StopSequence, string StopName, string CustomerName,
    DateTimeOffset? PlannedArrivalFrom, DateTimeOffset? PlannedArrivalTo,
    DateTimeOffset? ActualArrivalAt, DateTimeOffset? ActualDepartureAt,
    double? DwellMinutes, string StopStatus,
    int DeliveryCount, int DeliveredCount, int FailedDeliveryCount, int PartialDeliveryCount);

// Aggregated one row per (transporter, driver, customer). Only stops carrying BOTH a planned window
// end and an actual arrival are evaluated — an unplanned or unvisited stop is neither on time nor late.
public readonly record struct TripOnTimePerformanceRowVm(
    string TransporterName, string DriverName, string CustomerName,
    int EvaluatedStopCount, int OnTimeStopCount, int DelayedStopCount,
    double OnTimePercent, double AverageDelayMinutes, double MaxDelayMinutes);

// Dwell distribution aggregated per (stop, customer); only stops with both an arrival and a
// departure contribute, so an open visit never deflates the average.
public readonly record struct TripStopDwellRowVm(
    string StopName, string CustomerName, int VisitCount,
    double AverageDwellMinutes, double MinDwellMinutes, double MaxDwellMinutes, double TotalDwellMinutes);

// One row per matched station. PartialNoTariff makes a catalog gap visible instead of netting the
// missing tariff to zero (spec 11 §7.7 / §13).
public readonly record struct TripTollCostRowVm(
    string TripCode, string RoutePlanId, string TollStationName,
    string RoadName, string Direction, string TollVehicleClass,
    decimal? EstimatedTollAmount, string TollCurrency, bool PartialNoTariff);

// Sensitive export (spec 06 §13 / SC-06): carries receiver identity and capture coordinates.
public readonly record struct TripPodExportRowVm(
    string TripCode, int StopSequence, string StopName,
    string ReceiverName, string ReceiverDocument,
    DateTimeOffset CapturedAt, double? Latitude, double? Longitude,
    int DocumentCount);
