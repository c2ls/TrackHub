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

namespace TrackHub.Router.Infrastructure.Webfleet.Mappers;

internal static class PositionMapper
{
    private const double MicrodegreesPerDegree = 1_000_000d;
    private const double MetersPerOdometerUnit = 100d;
    private const double SecondsPerHour = 3600d;

    public static bool HasPosition(this ObjectReport report)
        => report is { Pos_time: not null, Latitude_mdeg: not null, Longitude_mdeg: not null };

    public static bool HasPosition(this TrackPoint point)
        => point is { Pos_time: not null, Latitude: not null, Longitude: not null };

    public static PositionVm MapToPositionVm(this ObjectReport report, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            ToDegrees(report.Latitude_mdeg),
            ToDegrees(report.Longitude_mdeg),
            null,
            ToUtc(report.Pos_time),
            null,
            report.Speed ?? 0,
            report.Course,
            null,
            report.Postext,
            null,
            null,
            null,
            new AttributesVm
            {
                Ignition = report.Ignition switch { 1 => true, 0 => false, _ => null },
                Mileage = report.Odometer_long ?? report.Odometer * MetersPerOdometerUnit,
                Hourmeter = report.Engine_operation_time / SecondsPerHour
            }
        );

    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<ObjectReport> reports, IDictionary<string, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var report in reports)
        {
            if (report.HasPosition() && devicesDictionary.TryGetValue(report.Objectno, out var device))
            {
                yield return report.MapToPositionVm(device);
            }
        }
    }

    public static PositionVm MapToPositionVm(this TrackPoint point, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            ToDegrees(point.Latitude),
            ToDegrees(point.Longitude),
            null,
            ToUtc(point.Pos_time),
            point.Receivetime?.ToUniversalTime(),
            point.Speed ?? 0,
            point.Course,
            null,
            null,
            null,
            point.State,
            point.Country,
            point.Odometer is null ? null : new AttributesVm { Mileage = point.Odometer * MetersPerOdometerUnit }
        );

    private static double ToDegrees(long? microdegrees)
        => (microdegrees ?? 0) / MicrodegreesPerDegree;

    private static DateTimeOffset ToUtc(DateTimeOffset? value)
        => value?.ToUniversalTime() ?? DateTimeOffset.MinValue;
}
