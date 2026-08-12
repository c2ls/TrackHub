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

namespace TrackHub.Router.Infrastructure.Traccar.Mappers;

internal static class DeviceMapper
{
    // Default device type and transporter type if not provided
    const DeviceType DefaultDeviceType = DeviceType.Cellular;
    const TransporterType DefaultTransporterType = TransporterType.Truck;

    // Maps a Device object and a DeviceVm object to an DeviceVm object
    public static DeviceVm MapToDeviceVm(this Device device, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            device.Id,
            device.UniqueId,
            device.Name,
            (short)DefaultDeviceType,
            (short)DefaultTransporterType
        );

    // Maps a Device object to an DeviceVm object with null DeviceId
    public static DeviceVm MapToDeviceVm(this Device device)
        => new(
            null,
            device.Id,
            device.UniqueId,
            device.Name,
            (short)DefaultDeviceType,
            (short)DefaultTransporterType
        );

    // Maps a collection of Device objects to a collection of DeviceVm objects using a dictionary of DeviceVm objects
    public static IEnumerable<DeviceVm> MapToDeviceVm(this IEnumerable<Device> devices, IDictionary<int, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var device in devices)
        {
            if (devicesDictionary.TryGetValue(device.Id, out var selectedDevice))
            {
                yield return device.MapToDeviceVm(selectedDevice);
            }
            else
            {
                continue;
            }
        }
    }

    // Maps a collection of Device objects to a collection of ExternalDeviceVm objects
    public static IEnumerable<DeviceVm> MapToDeviceVm(this IEnumerable<Device> devices)
    {
        foreach (var device in devices)
        {
            yield return device.MapToDeviceVm();
        }
    }
}
