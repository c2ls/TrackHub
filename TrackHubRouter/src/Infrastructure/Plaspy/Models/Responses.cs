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

namespace TrackHub.Router.Infrastructure.Plaspy.Models;

internal interface IPlaspyResponse
{
    bool? Success { get; }
    string? Error { get; }
}

internal sealed record AuthResult(bool Success, int? ErrorCode, string? ErrorText, string? Error, string? Token, string? Expires);

internal sealed record DevicesResponse(bool? Success, string? Error, IReadOnlyList<Device>? Devices) : IPlaspyResponse;

internal sealed record DeviceResponse(bool? Success, string? Error, Device? Device) : IPlaspyResponse;

internal sealed record LastLocationResponse(bool? Success, string? Error, Location? LastLocation) : IPlaspyResponse;

internal sealed record LocationsResponse(bool? Success, string? Error, IReadOnlyList<Location>? Locations) : IPlaspyResponse;

internal sealed record Device(string? Id, string? Name);

internal sealed record Location(
    string DateTime,
    double Latitude,
    double Longitude,
    double? Speed,
    double? Course,
    double? Temperature,
    double? Milleage);
