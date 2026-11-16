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

namespace TrackHub.Router.Infrastructure.Webfleet.Models;

internal sealed record ObjectReport(
    string Objectno,
    string? Objectname,
    string? Objectuid,
    [property: JsonConverter(typeof(LenientBoolConverter))] bool? Deleted,
    DateTimeOffset? Pos_time,
    long? Latitude_mdeg,
    long? Longitude_mdeg,
    double? Speed,
    double? Course,
    string? Postext,
    int? Ignition,
    long? Odometer,
    long? Odometer_long,
    long? Engine_operation_time
);
