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

using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Application.Report.Factory.Trip;

// Shared filter parsing and punctuality/dwell math for the six spec 11 §13 trip reports.
internal static class TripReportSupport
{
    // Optional GUID filter slots arrive as free text — an unparseable value means "no filter" rather
    // than an error, matching the other catalog reports' filter tolerance.
    public static Guid? ParseOptionalId(string? value)
        => Guid.TryParse(value, out var id) && id != Guid.Empty ? id : null;

    public static string OrEmpty(this string? value) => value ?? string.Empty;

    // Missing text groups under one explicit bucket rather than several empty-looking ones.
    public static string OrUnspecified(this string? value)
        => string.IsNullOrWhiteSpace(value) ? Unspecified : value;

    public const string Unspecified = "-";

    public static double? ToKilometers(this double? meters) => meters is { } m ? m / 1000d : null;

    public static double ToKilometers(this double meters) => meters / 1000d;

    // Minutes between a stop's arrival and departure; null while the visit is still open.
    public static double? DwellMinutes(DateTimeOffset? arrivedAt, DateTimeOffset? departedAt)
        => arrivedAt is { } arrival && departedAt is { } departure && departure >= arrival
            ? (departure - arrival).TotalMinutes
            : null;

    // Minutes late against the end of the planned window; null when the stop is not evaluable
    // (no planned window, or never arrived). Negative means early — clamped to zero by the caller
    // when it aggregates delay, so an early arrival never offsets someone else's lateness.
    public static double? DelayMinutes(DateTimeOffset? plannedArrivalTo, DateTimeOffset? actualArrivalAt)
        => plannedArrivalTo is { } planned && actualArrivalAt is { } actual
            ? (actual - planned).TotalMinutes
            : null;

    // A trip is on time when it finished no later than planned. Null (not false) when either end is
    // unknown — an unfinished or unplanned trip is not a punctuality failure.
    public static bool? IsOnTime(this ReportTripVm trip)
        => trip.PlannedEndAt is { } planned && trip.ActualEndAt is { } actual ? actual <= planned : null;
}
