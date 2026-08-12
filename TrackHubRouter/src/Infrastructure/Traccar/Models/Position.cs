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

namespace TrackHub.Router.Infrastructure.Traccar.Models;

internal readonly record struct Position(
    int Id,
    int DeviceId,
    string Protocol,
    // Traccar sends ISO 8601 with an explicit offset; honour it and normalize to UTC.
    [property: JsonConverter(typeof(UtcDateTimeOffsetJsonConverter))] DateTimeOffset DeviceTime,
    [property: JsonConverter(typeof(UtcDateTimeOffsetJsonConverter))] DateTimeOffset FixTime,
    [property: JsonConverter(typeof(UtcDateTimeOffsetJsonConverter))] DateTimeOffset ServerTime,
    bool Outdated,
    bool Valid,
    double Latitude,
    double Longitude,
    double Altitude,
    double Speed,
    double Course,
    string Address,
    double Accuracy,
    PositionAttribute Attributes
    );
