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
using TrackHub.Router.Domain.Helpers;

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
        var device = await WithReauthenticationAsync(
            () => HttpClientService.GetAsync<DevicePosition>($"DataConnectAPI/api/Device?id={deviceDto.Identifier}", Header, cancellationToken),
            cancellationToken);
        return DeviceMapper.Instance.Map(device, deviceDto);
    }

    // Retrieves a single device asynchronously
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesDictionary = devices.ToDeviceLookup(device => device.Identifier);
        var results = new List<DeviceVm>();
        foreach (var chunk in devicesDictionary.Values.Chunk(ProviderBatching.MaxIdsPerRequest))
        {
            var ids = chunk.GetIdsQueryString();
            var result = await WithReauthenticationAsync(
                () => HttpClientService.GetAsync<IEnumerable<DevicePosition>>($"DataConnectAPI/api/Devices?{ids}", Header, cancellationToken),
                cancellationToken);
            if (result is not null)
            {
                results.AddRange(DeviceMapper.Instance.Map(result, devicesDictionary));
            }
        }
        return results;
    }

    // Retrieves multiple devices asynchronously
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var devices = await WithReauthenticationAsync(
            () => HttpClientService.GetAsync<IEnumerable<DevicePosition>>("DataConnectAPI/api/AllDevices", Header, cancellationToken),
            cancellationToken);
        return devices is null ? [] : DeviceMapper.Instance.Map(devices).Distinct();
    }
}
