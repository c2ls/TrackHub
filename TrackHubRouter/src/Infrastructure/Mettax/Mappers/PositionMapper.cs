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

using System.Globalization;
using Common.Domain.Helpers;
using TrackHub.Router.Domain.Helpers;

namespace TrackHub.Router.Infrastructure.Mettax.Mappers;

internal static class PositionMapper
{
    // The fix time is the device's report time; DeviceTime is when Mettax stored it.
    public static PositionVm? MapToPositionVm(this Fix fix, string? reportTime, DeviceTransporterVm deviceDto)
        => GpsFix.IsValid(fix.Lat, fix.Lon) && UtcTimestamps.Parse(reportTime ?? fix.DeviceTime) is { } deviceDateTime
            ? new PositionVm(
                deviceDto.TransporterId,
                deviceDto.Name,
                deviceDto.TransporterType,
                fix.Lat!.Value,
                fix.Lon!.Value,
                null,
                deviceDateTime,
                UtcTimestamps.Parse(fix.DeviceTime),
                fix.Speed ?? 0,
                fix.Course,
                null,
                fix.Address,
                null,
                null,
                null,
                new AttributesVm(
                    fix.Acc,
                    (int?)fix.Num,
                    fix.Odometer * Units.MetersPerKilometer,
                    null,
                    null,
                    AttributesExtra.From(
                        ("battery", fix.Quantity?.ToString(CultureInfo.InvariantCulture)),
                        ("gpsFix", PositioningType(fix.Type)))))
            : null;

    private static string? PositioningType(double? type)
        => type switch { 1 => "gps", 2 => "lbs", 3 => "wifi", _ => null };
}
