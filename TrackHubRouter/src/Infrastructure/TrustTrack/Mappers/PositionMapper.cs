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

namespace TrackHub.Router.Infrastructure.TrustTrack.Mappers;

internal static class PositionMapper
{
    public static string FormatUtc(DateTimeOffset instant)
        => Uri.EscapeDataString(UtcTimestamps.Format(instant, UtcTimestamps.Iso8601Milliseconds));

    public static PositionVm? MapToPositionVm(this TrackedObject trackedObject, DeviceTransporterVm deviceDto)
        => trackedObject.LastCoordinate is { Datetime: { } deviceDateTime } coordinate
            ? coordinate.MapToPositionVm(deviceDateTime, deviceDto, null, null, null)
            : null;

    public static PositionVm? MapToPositionVm(this Coordinate coordinate, DeviceTransporterVm deviceDto)
        => coordinate is { Position: { } position, Datetime: { } deviceDateTime }
            ? position.MapToPositionVm(
                deviceDateTime,
                deviceDto,
                coordinate.Ignition,
                coordinate.Inputs?.CalculatedInputs?.Mileage * Units.MetersPerKilometer,
                coordinate.Address)
            : null;

    private static PositionVm? MapToPositionVm(
        this Position position,
        DateTimeOffset deviceDateTime,
        DeviceTransporterVm deviceDto,
        bool? ignition,
        double? mileage,
        Address? address)
        => GpsFix.IsValid(position.Latitude, position.Longitude)
            ? new PositionVm(
                deviceDto.TransporterId,
                deviceDto.Name,
                deviceDto.TransporterType,
                position.Latitude!.Value,
                position.Longitude!.Value,
                position.Altitude,
                deviceDateTime.ToUniversalTime(),
                null,
                position.Speed ?? 0,
                position.Direction,
                null,
                address is null ? null : string.Join(' ', new[] { address.Street, address.HouseNumber }.Where(part => !string.IsNullOrWhiteSpace(part))),
                address?.Locality,
                null,
                address?.Country,
                new AttributesVm(ignition, (int?)position.SatellitesCount, mileage, null, null))
            : null;
}
