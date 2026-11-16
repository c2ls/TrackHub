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

namespace TrackHub.Router.Infrastructure.Mapon.Mappers;

internal static class PositionMapper
{
    public static string FormatUtc(DateTimeOffset instant)
        => Uri.EscapeDataString(UtcTimestamps.Format(instant, UtcTimestamps.Iso8601));

    public static PositionVm? MapToPositionVm(this Unit unit, DeviceTransporterVm deviceDto)
        => GpsFix.IsValid(unit.Lat, unit.Lng) && unit.LastUpdate is { } deviceDateTime
            ? new PositionVm(
                deviceDto.TransporterId,
                deviceDto.Name,
                deviceDto.TransporterType,
                unit.Lat!.Value,
                unit.Lng!.Value,
                unit.Altitude?.Value,
                deviceDateTime.ToUniversalTime(),
                null,
                unit.Speed ?? 0,
                unit.Direction,
                null,
                null,
                null,
                null,
                null,
                new AttributesVm(
                    unit.Ignition?.Value,
                    null,
                    unit.Mileage,
                    null,
                    null))
            : null;

    public static PositionVm? MapToPositionVm(this RoutePoint point, DeviceTransporterVm deviceDto)
        => GpsFix.IsValid(point.Lat, point.Lng) && point.Gmt is { } deviceDateTime
            ? new PositionVm(
                deviceDto.TransporterId,
                deviceDto.Name,
                deviceDto.TransporterType,
                point.Lat!.Value,
                point.Lng!.Value,
                null,
                deviceDateTime.ToUniversalTime(),
                null,
                point.Speed ?? 0,
                null,
                null,
                null,
                null,
                null,
                null,
                null)
            : null;
}
