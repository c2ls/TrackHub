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

using Common.Domain.Helpers;
using TrackHub.Router.Domain.Helpers;

namespace TrackHub.Router.Infrastructure.GpsTrackit.Mappers;

internal static class PositionMapper
{
    public static PositionVm? MapToPositionVm(this LastReading reading, DeviceTransporterVm deviceDto)
        => Map(reading.Latitude, reading.Longitude, UtcTimestamps.FromUnixEpoch(reading.UnitTime), reading.Speed, reading.Heading,
            reading.EngineOdometer, null, null, reading.EventName, reading.Address, null, deviceDto);

    public static PositionVm? MapToPositionVm(this Reading reading, DeviceTransporterVm deviceDto)
        => Map(reading.Latitude, reading.Longitude, reading.UnitTime, reading.Speed, reading.Heading,
            reading.EngineOdometer, reading.IgnitionStatus, reading.SatelliteCount, reading.EventName, null, reading.FormattedAddress, deviceDto);

    private static PositionVm? Map(
        double? latitude,
        double? longitude,
        DateTimeOffset? unitTime,
        double? speedMph,
        double? heading,
        double? odometerMiles,
        bool? ignition,
        double? satellites,
        string? eventName,
        ReadingAddress? address,
        string? formattedAddress,
        DeviceTransporterVm deviceDto)
        => GpsFix.IsValid(latitude, longitude) && unitTime is { } deviceDateTime
            ? new PositionVm(
                deviceDto.TransporterId,
                deviceDto.Name,
                deviceDto.TransporterType,
                latitude!.Value,
                longitude!.Value,
                null,
                deviceDateTime.ToUniversalTime(),
                null,
                (speedMph ?? 0) * Units.KilometersPerMile,
                heading,
                null,
                address?.Formatted ?? formattedAddress,
                address?.City,
                address?.State,
                address?.Country,
                new AttributesVm(
                    ignition,
                    (int?)satellites,
                    odometerMiles * Units.MetersPerMile,
                    null,
                    null,
                    AttributesExtra.From(("event", eventName))))
            : null;
}
