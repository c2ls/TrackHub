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

namespace TrackHub.Router.Infrastructure.Pegasus.Models;

internal sealed record LoginResponse(string? Message, string? Auth);

internal sealed record PagedResponse<T>(IReadOnlyList<T>? Data, int Page, int Pages);

// Without a select, "device" is the assigned IMEI (null when the vehicle has no device).
internal sealed record Vehicle(long Id, string? Name, long? Device)
{
    public bool HasDevice => Device is > 0;
}

// With select=device:latest..., "device" becomes an object carrying the IMEI and the latest state.
internal sealed record VehicleLatest(long Id, string? Name, LatestDevice? Device);

internal sealed record LatestDevice(long Imei, DeviceLatest? Latest);

internal sealed record DeviceLatest(LatestLocation? Loc, LatestIos? Ios, LatestCounters? Counters);

internal sealed record LatestLocation(
    double Lat,
    double Lon,
    double? Head,
    double? Mph,
    double Evtime,
    double? Systime,
    bool? Valid = null);

internal sealed record LatestIos(bool? Io_ign);

internal sealed record LatestCounters(double? Dev_dist, double? Dev_ign);

internal sealed record RawdataResponse(IReadOnlyList<RawEvent>? Events);

internal sealed record RawEvent(
    string Event_time,
    string? System_time,
    double? Lat,
    double? Lon,
    double? Al,
    double? Mph,
    double? Head,
    int? Sv,
    bool? Io_ign,
    double? Dev_dist,
    double? Dev_ign,
    double? Temp);
