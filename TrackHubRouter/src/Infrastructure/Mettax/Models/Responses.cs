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

using System.Text.Json.Serialization;
using TrackHub.Router.Domain.Helpers;

namespace TrackHub.Router.Infrastructure.Mettax.Models;

internal sealed record MettaxResponse<T>(int Code, string? Msg, T? Data);

internal sealed record Page<T>(IReadOnlyList<T>? Records, long? Total);

internal sealed record Device(string? Id, string? DeviceName);

internal sealed record DeviceShadow(Fix? DeviceData, ShadowExpand? Expand);

internal sealed record ShadowExpand(string? ReportTime);

// The shadow and history endpoints share these fields; the history adds ReportTime, the shadow Address and Odometer.
internal sealed record Fix(
    string? DeviceId,
    string? ReportTime,
    string? DeviceTime,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Lat,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Lon,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Speed,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Course,
    [property: JsonConverter(typeof(LenientBoolConverter))] bool? Acc,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Num,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Quantity,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Type,
    string? Address,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Odometer);
