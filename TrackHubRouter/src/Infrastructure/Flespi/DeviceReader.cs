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

using TrackHub.Router.Infrastructure.Flespi.Mappers;
using TrackHub.Router.Domain.Interfaces;

namespace TrackHub.Router.Infrastructure.Flespi;

/// <summary>
/// Reader for retrieving device information from Flespi API.
/// </summary>
public sealed class DeviceReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService)
    : FlespiReaderBase(httpClientFactory, httpClientService), IExternalDeviceReader
{
    /// <summary>
    /// Retrieves a single device asynchronously based on the provided device DTO.
    /// </summary>
    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var url = $"gw/devices/{deviceDto.Identifier}";
        var result = await HttpClientService.GetAsync<DeviceListResponse>(url, cancellationToken: cancellationToken);
        
        var device = result?.Result?.FirstOrDefault();
        return device is null
            ? throw new InvalidOperationException($"Device not found: {deviceDto.Identifier}")
            : device.Value.MapToDeviceVm(deviceDto);
    }

    /// <summary>
    /// Retrieves multiple devices asynchronously based on the provided device DTOs.
    /// </summary>
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var url = "gw/devices/all";
        var result = await HttpClientService.GetAsync<DeviceListResponse>(url, cancellationToken: cancellationToken);
        
        if (result?.Result is null || result.Result.Count == 0)
        {
            return [];
        }

        var devicesDictionary = devices.ToDictionary(device => device.Identifier, device => device);
        return result.Result
            .Where(d => devicesDictionary.ContainsKey((int)d.Id))
            .MapToDeviceVm(devicesDictionary);
    }

    /// <summary>
    /// Retrieves all devices asynchronously.
    /// </summary>
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var url = "gw/devices/all";
        var result = await HttpClientService.GetAsync<DeviceListResponse>(url, cancellationToken: cancellationToken);

        return result?.Result is null ? [] : result.Result.MapToDeviceVm().Distinct();
    }
}
