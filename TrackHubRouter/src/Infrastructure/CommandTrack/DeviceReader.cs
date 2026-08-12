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

using TrackHub.Router.Domain.Extensions;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Infrastructure.CommandTrack.Mappers;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Interfaces.Operator;

namespace TrackHub.Router.Infrastructure.CommandTrack;

// This class represents a device reader that retrieves device information from CommandTrack API
public sealed class DeviceReader(ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore
    ) : CommandTrackReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IExternalDeviceReader
{
    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var url = $"DataConnectAPI/api/Device?id={deviceDto.Identifier}";
        var device = await HttpClientService.GetAsync<DevicePosition>(url, Header, cancellationToken);
        return device.MapToDeviceVm(deviceDto);
    }

    // Retrieves a single device asynchronously
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var url = $"DataConnectAPI/api/Devices{devices.GetIdsQueryString()}";
        var result = await HttpClientService.GetAsync<IEnumerable<DevicePosition>>(url, Header, cancellationToken);
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
        var url = "DataConnectAPI/api/AllDevices";
        var devices = await HttpClientService.GetAsync<IEnumerable<DevicePosition>>(url, Header, cancellationToken);
        return devices is null ? [] : devices.MapToDeviceVm().Distinct();
    }
}
