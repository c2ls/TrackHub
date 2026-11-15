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
using Common.Domain.Enums;

namespace TrackHub.Router.Infrastructure.Pegasus.Mappers;

internal static class DeviceMapper
{
    private const DeviceType DefaultDeviceType = DeviceType.Cellular;
    private const TransporterType DefaultTransporterType = TransporterType.Truck;

    public static string ToSerial(long imei) => imei.ToString(CultureInfo.InvariantCulture);

    public static int ToIdentifier(long vehicleId) => vehicleId is > 0 and <= int.MaxValue ? (int)vehicleId : 0;

    public static bool HasDevice(this Vehicle vehicle) => vehicle.Device is > 0;

    public static DeviceVm MapToDeviceVm(this Vehicle vehicle, Guid? deviceId = null)
    {
        var serial = ToSerial(vehicle.Device!.Value);
        return new(
            deviceId,
            ToIdentifier(vehicle.Id),
            serial,
            vehicle.Name ?? serial,
            (short)DefaultDeviceType,
            (short)DefaultTransporterType);
    }

    public static IEnumerable<DeviceVm> MapToDeviceVm(this IEnumerable<Vehicle> vehicles)
        => vehicles.Where(HasDevice).Select(vehicle => vehicle.MapToDeviceVm());

    public static IEnumerable<DeviceVm> MapToDeviceVm(this IEnumerable<Vehicle> vehicles, IDictionary<string, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var vehicle in vehicles.Where(HasDevice))
        {
            if (devicesDictionary.TryGetValue(ToSerial(vehicle.Device!.Value), out var device))
            {
                yield return vehicle.MapToDeviceVm(device.TransporterId);
            }
        }
    }
}
