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

namespace TrackHub.Router.Infrastructure.Protrack.Models;

/// <summary>
/// Protrack track record (current position data for a device)
/// </summary>
internal readonly record struct TrackRecord(
    string Imei,
    long Servertime,
    long Gpstime,
    long Hearttime,
    long Systemtime,
    double Longitude,
    double Latitude,
    int Course,
    int Speed,
    long Acctime,
    int Accstatus,
    int Doorstatus,
    int Chargestatus,
    int Oilpowerstatus,
    int Defencestatus,
    int Datastatus,
    int Battery
);
