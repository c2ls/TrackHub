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

namespace TrackHub.Router.Infrastructure.Pegasus.Mappers;

internal static class PositionMapper
{
    private const double KmhPerMph = 1.609344;
    private const double SecondsPerHour = 3600;
    private const string RawdataTimeFormat = "yyyy-MM-ddTHH:mm:ss";

    public static DateTimeOffset FromEpochSeconds(double seconds)
        => DateTimeOffset.FromUnixTimeMilliseconds((long)Math.Round(seconds * 1000));

    // Rawdata answers in UTC (no tz parameter is sent), sometimes without the trailing Z.
    public static DateTimeOffset ParseUtc(string value)
        => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    public static string FormatUtc(DateTimeOffset instant)
        => instant.UtcDateTime.ToString(RawdataTimeFormat, CultureInfo.InvariantCulture);

    public static double ToKmh(double? mph) => (mph ?? 0) * KmhPerMph;

    public static double? ToHours(double? seconds) => seconds / SecondsPerHour;

    public static PositionVm MapToPositionVm(this DeviceLatest latest, LatestLocation location, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            location.Lat,
            location.Lon,
            null,
            FromEpochSeconds(location.Evtime),
            location.Systime is { } systime ? FromEpochSeconds(systime) : null,
            ToKmh(location.Mph),
            location.Head,
            null,
            null,
            null,
            null,
            null,
            new AttributesVm
            {
                Ignition = latest.Ios?.Io_ign,
                Mileage = latest.Counters?.Dev_dist,
                Hourmeter = ToHours(latest.Counters?.Dev_ign)
            });

    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<VehicleLatest> vehicles, IDictionary<string, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var vehicle in vehicles)
        {
            if (vehicle.Device is { Latest: { Loc: { Valid: not false } location } latest } device
                && devicesDictionary.TryGetValue(DeviceMapper.ToSerial(device.Imei), out var deviceDto))
            {
                yield return latest.MapToPositionVm(location, deviceDto);
            }
        }
    }

    public static PositionVm MapToPositionVm(this RawEvent rawEvent, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            rawEvent.Lat ?? 0,
            rawEvent.Lon ?? 0,
            rawEvent.Al,
            ParseUtc(rawEvent.Event_time),
            rawEvent.System_time is { } systemTime ? ParseUtc(systemTime) : null,
            ToKmh(rawEvent.Mph),
            rawEvent.Head,
            null,
            null,
            null,
            null,
            null,
            new AttributesVm
            {
                Ignition = rawEvent.Io_ign,
                Satellites = rawEvent.Sv,
                Mileage = rawEvent.Dev_dist,
                Hourmeter = ToHours(rawEvent.Dev_ign),
                Temperature = rawEvent.Temp
            });

    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<RawEvent> rawEvents, DeviceTransporterVm deviceDto)
        => rawEvents.Where(rawEvent => rawEvent is { Lat: not null, Lon: not null }).Select(rawEvent => rawEvent.MapToPositionVm(deviceDto));
}
