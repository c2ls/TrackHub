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

using TrackHub.Router.Infrastructure.Samsara.Mappers;
using TrackHub.Router.Domain.Interfaces;

namespace TrackHub.Router.Infrastructure.Samsara;

/// <summary>
/// Reader for retrieving device (vehicle) information from Samsara API.
/// </summary>
public sealed class DeviceReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService)
    : SamsaraReaderBase(httpClientFactory, httpClientService), IExternalDeviceReader
{
    /// <summary>
    /// Retrieves a single device asynchronously based on the provided device DTO.
    /// Uses the stats endpoint filtered by vehicle ID.
    /// </summary>
    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var url = $"fleet/vehicles/stats?vehicleIds={deviceDto.Serial}";
        var result = await HttpClientService.GetAsync<VehicleStatsResponse>(url, cancellationToken: cancellationToken);
        
        var vehicle = result?.Data?.FirstOrDefault();
        return vehicle is null
            ? throw new InvalidOperationException($"Device not found: {deviceDto.Serial}")
            : vehicle.Value.MapToDeviceVm(deviceDto);
    }

    /// <summary>
    /// Retrieves multiple devices asynchronously based on the provided device DTOs.
    /// </summary>
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var vehicleIds = string.Join(",", devices.Select(d => d.Serial));
        var url = $"fleet/vehicles/stats?vehicleIds={vehicleIds}";
        
        var result = await HttpClientService.GetAsync<VehicleStatsResponse>(url, cancellationToken: cancellationToken);
        if (result?.Data is null || !result.Data.Any())
        {
            return [];
        }

        var devicesDictionary = devices.ToDictionary(device => device.Serial, device => device);
        return result.Data.MapToDeviceVm(devicesDictionary);
    }

    /// <summary>
    /// Retrieves all devices asynchronously.
    /// </summary>
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var url = "fleet/vehicles/stats";
        var result = await HttpClientService.GetAsync<VehicleStatsResponse>(url, cancellationToken: cancellationToken);

        return result?.Data is null ? [] : result.Data.MapToDeviceVm().Distinct();
    }
}
