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

namespace TrackHub.Router.Infrastructure.Wialon.Models;

/// <summary>
/// Response from Wialon messages/load_interval API
/// </summary>
internal sealed record MessageResponse(
    IEnumerable<Message>? Messages,
    int Count,
    long? Error = null
) : IWialonResponse;

/// <summary>
/// Wialon message (historical position data)
/// T = timestamp, Tp = type, Pos = position, P = params
/// </summary>
internal readonly record struct Message(
    long T,              // timestamp
    string? Tp,          // type
    Position? Pos,       // position
    MessageParams? P     // params
);

/// <summary>
/// Message parameters containing sensor data
/// Odo = odometer, Pwr_ext = external power, Hdop = hdop
/// </summary>
internal readonly record struct MessageParams(
    double? Odo,         // odometer
    double? Pwr_ext,     // external power
    double? Hdop         // hdop
);
