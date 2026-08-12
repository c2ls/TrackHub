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

using Geotab.Checkmate.ObjectModel;
using TrackHub.Router.Infrastructure.Geotab.Mappers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Infrastructure.Geotab;

// This class represents a device reader that retrieves device information from the Geotab API
public sealed class DeviceReader(IProviderSessionStore sessionStore)
    : GeotabReaderBase(sessionStore), IExternalDeviceReader
{
    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var deviceSearch = new DeviceSearch(Id.Create(deviceDto.Identifier));
        var device = await Api.CallAsync<Device>("Get", typeof(Device), new { search = deviceSearch }, cancellationToken);
        PersistSession();
        return device!.MapToDeviceVm(deviceDto);
    }

    // Retrieves a single device asynchronously
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var deviceSearch = new DeviceSearch
        {
            DeviceIds = devices.Select(device => Id.Create(device.Identifier))
        };
        var result = await Api.CallAsync<IEnumerable<Device>>("Get", typeof(Device), new { search = deviceSearch }, cancellationToken);
        PersistSession();
        if (result is null)
        {
            return [];
        }
        var devicesDictionary = devices.ToDictionary(device => device.Identifier, device => device);
        return result.MapToDeviceVm(devicesDictionary);
    }

    // Retrieves multiple devices asynchronously
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var devices = await Api.CallAsync<IEnumerable<Device>>("Get", typeof(Device), cancellationToken);
        PersistSession();
        return devices is null ? ([]) : devices.MapToDeviceVm().Distinct();
    }
}
