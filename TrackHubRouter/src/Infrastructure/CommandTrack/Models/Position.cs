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
using Common.Domain.Json;

namespace TrackHub.Router.Infrastructure.CommandTrack.Models;

internal readonly record struct Position
(
    int PositionId,
    string Serial,
    string Plate,
    double Latitude,
    double Longitude,
    double? Altitude,
    // CommandTrack timestamps: assume UTC for naive values, honour any offset, always normalize to UTC.
    [property: JsonConverter(typeof(UtcDateTimeOffsetJsonConverter))] DateTimeOffset DeviceDateTime,
    [property: JsonConverter(typeof(UtcDateTimeOffsetJsonConverter))] DateTimeOffset ServerDateTime,
    double Speed,
    double Course,
    int EventId,
    string Address,
    double DistanceToAddress,
    string City,
    string State,
    string Country,
    bool Ignition,
    double? Satellites,
    double? Mileage,
    double? Temperature
);
