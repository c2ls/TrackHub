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

namespace TrackHub.Router.Infrastructure.Navixy.Models;

/// <summary>
/// Navixy tracker (device) model
/// Tracker_id = device id, Imei = unique identifier, Label = device name
/// </summary>
internal readonly record struct Tracker(
    long Tracker_id,
    string Imei,
    string Label,
    TrackerLastUpdate? Last_update
);

/// <summary>
/// Last known state of the tracker
/// </summary>
internal readonly record struct TrackerLastUpdate(
    string? Time,
    double? Lat,
    double? Lng,
    int? Speed,
    int? Heading
);
