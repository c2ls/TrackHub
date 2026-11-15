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

using Common.Domain.Enums;

namespace TrackHub.Router.Infrastructure.Plaspy.Mappers;

internal static class DeviceMapper
{
    private const DeviceType DefaultDeviceType = DeviceType.Cellular;
    private const TransporterType DefaultTransporterType = TransporterType.Truck;

    public static DeviceVm MapToDeviceVm(this Device device, Guid? deviceId = null)
        => new(
            deviceId,
            0,
            device.Id!,
            device.Name ?? device.Id!,
            (short)DefaultDeviceType,
            (short)DefaultTransporterType);

    public static IEnumerable<DeviceVm> MapToDeviceVm(this IEnumerable<Device> devices)
        => devices.Where(device => !string.IsNullOrEmpty(device.Id)).Select(device => device.MapToDeviceVm());

    public static IEnumerable<DeviceVm> MapToDeviceVm(this IEnumerable<Device> devices, IDictionary<string, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var device in devices)
        {
            if (device.Id is not null && devicesDictionary.TryGetValue(device.Id, out var selected))
            {
                yield return device.MapToDeviceVm(selected.TransporterId);
            }
        }
    }
}
