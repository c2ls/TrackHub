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

namespace TrackHub.Router.Infrastructure.GpsGate.Models;

internal readonly record struct Device(
    int Id,
    string Name,
    string IMEI,
    double Latitude,
    double Longitude,
    double? Altitude,
    // GpsGate timestamp: assume UTC for naive values, honour any offset, always normalize to UTC.
    [property: JsonConverter(typeof(UtcNullableDateTimeOffsetJsonConverter))] DateTimeOffset? TimeStamp
    );
