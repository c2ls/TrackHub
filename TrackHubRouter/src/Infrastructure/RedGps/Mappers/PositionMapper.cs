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

namespace TrackHub.Router.Infrastructure.RedGps.Mappers;

internal static class PositionMapper
{
    private const string RequestDateFormat = "yyyy-MM-dd HH:mm:ss";
    private const string HistoryDateFormat = "yyyyMMddHHmmss";

    // The docs do not say in which zone fechaIni/fechaFin are read (history points carry their own gmt), so the
    // requested window is widened by the largest UTC offset on both sides and points are filtered by their UTC time.
    internal static readonly TimeSpan RequestZonePadding = TimeWindows.LargestUtcOffset;

    private static readonly Dictionary<string, double> CompassDegrees = new(StringComparer.OrdinalIgnoreCase)
    {
        ["N"] = 0, ["NE"] = 45, ["E"] = 90, ["SE"] = 135, ["S"] = 180,
        ["SO"] = 225, ["SW"] = 225, ["O"] = 270, ["W"] = 270, ["NO"] = 315, ["NW"] = 315
    };

    public static (string FechaIni, string FechaFin) RequestWindow(DateTimeOffset start, DateTimeOffset end)
        => ((start - RequestZonePadding).UtcDateTime.ToString(RequestDateFormat, CultureInfo.InvariantCulture),
            (end + RequestZonePadding).UtcDateTime.ToString(RequestDateFormat, CultureInfo.InvariantCulture));

    public static DateTimeOffset? ParseUtcReportDate(string? value)
        => DateTimeOffset.TryParseExact(value, RequestDateFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)
            ? parsed
            : null;

    public static DateTimeOffset ParseHistoryTime(HistoryPoint point)
    {
        if (!DateTime.TryParseExact(point.Fecha_reporte, HistoryDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var wallClock))
        {
            throw new InvalidOperationException($"RedGPS history point has an unreadable fecha_reporte '{point.Fecha_reporte}'.");
        }
        if (!double.TryParse(point.Gmt, NumberStyles.Float, CultureInfo.InvariantCulture, out var gmtHours))
        {
            throw new InvalidOperationException($"RedGPS history point has an unreadable gmt '{point.Gmt}'.");
        }
        return new DateTimeOffset(wallClock, TimeSpan.FromMinutes(Math.Round(gmtHours * 60))).ToUniversalTime();
    }

    public static double? ParseDirection(string? value)
        => value is null ? null
            : CompassDegrees.TryGetValue(value.Trim(), out var degrees) ? degrees
            : double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric) ? numeric
            : null;

    // 0 = off, 1 = on, 2 = unknown.
    public static bool? ParseIgnition(double? value)
        => value switch
        {
            1 => true,
            0 => false,
            _ => null
        };

    public static PositionVm MapToPositionVm(this UnitData unit, DateTimeOffset deviceTime, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            unit.Latitude ?? 0,
            unit.Longitude ?? 0,
            unit.Altitude,
            deviceTime,
            null,
            unit.GpsSpeed ?? 0,
            ParseDirection(unit.Direction),
            null,
            unit.Domicilio,
            null,
            null,
            null,
            new AttributesVm
            {
                Ignition = ParseIgnition(unit.Ignition),
                Satellites = (int?)unit.Satellites,
                Mileage = unit.Odometer
            }
        );

    public static PositionVm MapToPositionVm(this HistoryPoint point, DateTimeOffset deviceTime, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            point.Latitud ?? 0,
            point.Longitud ?? 0,
            null,
            deviceTime,
            null,
            point.Velocidad ?? 0,
            point.Orientacion,
            null,
            point.Domicilio,
            null,
            null,
            null,
            new AttributesVm
            {
                Ignition = ParseIgnition(point.Ignicion),
                Satellites = (int?)point.Satelites,
                Mileage = point.Odometro
            }
        );

    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<UnitData> units, IDictionary<string, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var unit in units)
        {
            if (unit.GpsIdentif is not null
                && GpsFix.IsValid(unit.Latitude, unit.Longitude)
                && devicesDictionary.TryGetValue(unit.GpsIdentif, out var device)
                && ParseUtcReportDate(unit.ReportDate) is { } deviceTime)
            {
                yield return unit.MapToPositionVm(deviceTime, device);
            }
        }
    }

    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<HistoryPoint> points, DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto)
    {
        foreach (var point in points)
        {
            var deviceTime = ParseHistoryTime(point);
            if (GpsFix.IsValid(point.Latitud, point.Longitud) && deviceTime >= from && deviceTime <= to)
            {
                yield return point.MapToPositionVm(deviceTime, deviceDto);
            }
        }
    }
}
