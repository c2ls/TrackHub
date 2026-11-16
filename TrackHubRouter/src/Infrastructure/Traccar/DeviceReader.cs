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

using TrackHub.Router.Infrastructure.Traccar.Mappers;
using TrackHub.Router.Domain.Extensions;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Helpers;

namespace TrackHub.Router.Infrastructure.Traccar;


// This class represents a reader for Traccar api - devices.
public sealed class DeviceReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService)
    : TraccarReaderBase(httpClientFactory, httpClientService), IExternalDeviceReader
{

    /// <summary>
    /// Retrieves a single device asynchronously based on the provided device DTO.
    /// </summary>
    /// <param name="deviceDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the device as an DeviceVm.</returns>
    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var url = $"api/devices?id={deviceDto.Identifier}";
        // Traccar answers `api/devices?id=` with an ARRAY even for one id; reading it as a single
        // object failed to deserialise.
        var devices = await HttpClientService.GetAsync<IEnumerable<Device>>(url, cancellationToken: cancellationToken);
        var device = devices?.FirstOrDefault()
            ?? throw new InvalidOperationException($"Traccar returned no device for id {deviceDto.Identifier}.");
        return DeviceMapper.Instance.Map(device, deviceDto);
    }

    /// <summary>
    /// Retrieves multiple devices asynchronously based on the provided device DTOs.
    /// </summary>
    /// <param name="devices"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the devices as a collection of DeviceVm.</returns>
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesDictionary = devices.ToDeviceLookup(device => device.Identifier);
        var results = new List<DeviceVm>();
        foreach (var chunk in devicesDictionary.Values.Chunk(ProviderBatching.MaxIdsPerRequest))
        {
            var url = $"api/devices?{chunk.GetIdsQueryString()}";
            var result = await HttpClientService.GetAsync<IEnumerable<Device>>(url, cancellationToken: cancellationToken);
            if (result is not null)
            {
                results.AddRange(DeviceMapper.Instance.Map(result, devicesDictionary));
            }
        }
        return results;
    }

    /// <summary>
    /// Retrieves all devices asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns all devices as a collection of DeviceVm.</returns>
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var url = "api/devices?all=true";
        var devices = await HttpClientService.GetAsync<IEnumerable<Device>>(url, cancellationToken: cancellationToken);
        return devices is null ? ([]) : DeviceMapper.Instance.Map(devices).Distinct();
    }
}
