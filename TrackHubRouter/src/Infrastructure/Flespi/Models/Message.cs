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

namespace TrackHub.Router.Infrastructure.Flespi.Models;

/// <summary>
/// Flespi message (contains GPS position data)
/// Timestamp = Unix timestamp in seconds
/// </summary>
internal readonly record struct Message(
    long? Ident,
    long? Device_id,
    long? Channel_id,
    double? Timestamp,
    double? Position_latitude,
    double? Position_longitude,
    double? Position_altitude,
    double? Position_speed,
    int? Position_direction,
    int? Position_satellites
);
