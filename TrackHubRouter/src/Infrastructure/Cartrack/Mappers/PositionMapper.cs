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

namespace TrackHub.Router.Infrastructure.Cartrack.Mappers;

internal static class PositionMapper
{
    private const double MinutesPerHour = 60;

    // An unconnected or silent temperature sensor reads 0, so 0 cannot be told apart from a real reading.
    private static double? Temperature(double? value) => value is 0 ? null : value;

    private static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;

    public static bool HasFix(this VehicleEvent vehicleEvent)
        => vehicleEvent is { Latitude: not null, Longitude: not null } && vehicleEvent.GpsFixType != 0;

    public static PositionVm MapToPositionVm(this VehicleEvent vehicleEvent, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            vehicleEvent.Latitude!.Value,
            vehicleEvent.Longitude!.Value,
            vehicleEvent.Altitude,
            vehicleEvent.EventTs,
            vehicleEvent.ReceivedTs,
            vehicleEvent.Speed ?? 0,
            vehicleEvent.Bearing,
            null,
            NullIfEmpty(vehicleEvent.PositionDescription),
            null,
            null,
            null,
            new AttributesVm(
                vehicleEvent.Ignition,
                null,
                vehicleEvent.Odometer,
                vehicleEvent.Clock / MinutesPerHour,
                Temperature(vehicleEvent.Temp1)));

    public static PositionVm MapToPositionVm(this VehicleStatus status, StatusLocation location, DateTimeOffset deviceTime, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            location.Latitude!.Value,
            location.Longitude!.Value,
            status.Altitude,
            deviceTime,
            null,
            status.Speed ?? 0,
            status.Bearing,
            null,
            NullIfEmpty(location.PositionDescription),
            null,
            null,
            null,
            new AttributesVm(
                status.Ignition,
                null,
                status.Odometer,
                status.Clock / MinutesPerHour,
                Temperature(status.Temp1)));

    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<VehicleStatus> statuses, IDictionary<string, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var status in statuses)
        {
            if (status.Location is { Latitude: not null, Longitude: not null } location
                && (location.Updated ?? status.EventTs) is { } deviceTime
                && devicesDictionary.TryGetValue(status.Registration, out var device))
            {
                yield return status.MapToPositionVm(location, deviceTime, device);
            }
        }
    }
}
