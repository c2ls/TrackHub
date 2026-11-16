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
using TrackHub.Router.Domain.Helpers;

namespace TrackHub.Router.Infrastructure.Tracksolid.Mappers;

internal static class PositionMapper
{
    private const double MetersPerKilometer = 1000;

    public static DateTimeOffset? ParseUtc(string? value)
        => DateTimeOffset.TryParseExact(value, TracksolidReaderBase.TimestampFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)
            ? parsed
            : null;

    public static string FormatUtc(DateTimeOffset instant)
        => instant.UtcDateTime.ToString(TracksolidReaderBase.TimestampFormat, CultureInfo.InvariantCulture);

    // accStatus is "1"/"0" on location reads; track points carry "ON"/"OFF".
    internal static bool? ParseIgnition(string? value)
        => value?.Trim().ToUpperInvariant() switch
        {
            "1" or "ON" => true,
            "0" or "OFF" => false,
            _ => null
        };

    // -1 means the device did not report a heading.
    private static double? Course(double? direction)
        => direction is >= 0 ? direction : null;

    public static PositionVm MapToPositionVm(this LocationRecord location, DateTimeOffset deviceTime, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            location.Lat ?? 0,
            location.Lng ?? 0,
            null,
            deviceTime,
            null,
            location.Speed ?? 0,
            Course(location.Direction),
            null,
            null,
            null,
            null,
            null,
            new AttributesVm
            {
                Ignition = ParseIgnition(location.AccStatus),
                Satellites = (int?)location.GpsNum,
                Mileage = location.CurrentMileage * MetersPerKilometer,
                Temperature = location.Temperature
            }
        );

    public static PositionVm MapToPositionVm(this TrackPoint point, DateTimeOffset deviceTime, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            point.Lat ?? 0,
            point.Lng ?? 0,
            null,
            deviceTime,
            null,
            point.GpsSpeed ?? 0,
            Course(point.Direction),
            null,
            null,
            null,
            null,
            null,
            new AttributesVm { Ignition = ParseIgnition(point.Ignition ?? point.AccStatus) }
        );

    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<LocationRecord> locations, IDictionary<string, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var location in locations)
        {
            if (GpsFix.IsValid(location.Lat, location.Lng)
                && ParseUtc(location.GpsTime) is { } deviceTime
                && devicesDictionary.TryGetValue(location.Imei, out var device))
            {
                yield return location.MapToPositionVm(deviceTime, device);
            }
        }
    }

    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<TrackPoint> points, DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto)
    {
        foreach (var point in points)
        {
            if (GpsFix.IsValid(point.Lat, point.Lng) && ParseUtc(point.GpsTime) is { } deviceTime && deviceTime >= from && deviceTime <= to)
            {
                yield return point.MapToPositionVm(deviceTime, deviceDto);
            }
        }
    }
}
