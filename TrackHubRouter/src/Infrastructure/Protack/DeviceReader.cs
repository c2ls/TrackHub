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

using TrackHub.Router.Infrastructure.Protrack.Mappers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Interfaces.Operator;

namespace TrackHub.Router.Infrastructure.Protrack;

/// <summary>
/// Reader for retrieving device information from Protrack API.
/// Uses /api/device/list for all devices and /api/device/detail for specific IMEIs.
/// </summary>
public sealed class DeviceReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
    : ProtrackReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IExternalDeviceReader
{
    /// <summary>
    /// Retrieves a single device asynchronously based on the provided device DTO.
    /// Uses /api/device/detail with the device's IMEI (Serial).
    /// </summary>
    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var url = $"{BaseUrl}/api/device/detail?access_token={AccessToken}&imeis={deviceDto.Serial}";
        var result = await HttpClientService.GetAsync<DeviceListResponse>(url, cancellationToken: cancellationToken);

        var device = result?.Record?.FirstOrDefault(d => d.Imei == deviceDto.Serial);
        return device is null
            ? throw new InvalidOperationException($"Device not found: {deviceDto.Serial}")
            : device.Value.MapToDeviceVm(deviceDto);
    }

    /// <summary>
    /// Retrieves multiple devices asynchronously based on the provided device DTOs.
    /// Uses /api/device/detail with comma-separated IMEIs (max 100).
    /// </summary>
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesList = devices.ToList();
        if (devicesList.Count == 0)
        {
            return [];
        }

        var imeis = string.Join(",", devicesList.Select(d => d.Serial));
        var url = $"{BaseUrl}/api/device/detail?access_token={AccessToken}&imeis={imeis}";
        var result = await HttpClientService.GetAsync<DeviceListResponse>(url, cancellationToken: cancellationToken);

        if (result?.Record is null || !result.Record.Any())
        {
            return [];
        }

        var devicesDictionary = devicesList.ToDictionary(device => device.Serial, device => device);
        return result.Record.MapToDeviceVm(devicesDictionary);
    }

    /// <summary>
    /// Retrieves all devices asynchronously.
    /// Uses /api/device/list to get all devices for the account.
    /// </summary>
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var url = $"{BaseUrl}/api/device/list?access_token={AccessToken}";
        var result = await HttpClientService.GetAsync<DeviceListResponse>(url, cancellationToken: cancellationToken);

        return result?.Record is null ? [] : result.Record.MapToDeviceVm().Distinct();
    }
}
