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

using TrackHub.Router.Infrastructure.Navixy.Mappers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Operator;

namespace TrackHub.Router.Infrastructure.Navixy;

/// <summary>
/// Reader for retrieving device (tracker) information from Navixy API.
/// </summary>
public sealed class DeviceReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    IProviderSessionStore sessionStore)
    : NavixyReaderBase(httpClientFactory, httpClientService, sessionStore), IExternalDeviceReader
{
    /// <summary>
    /// Retrieves a single device asynchronously based on the provided device DTO.
    /// </summary>
    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var result = await PostNavixyAsync<TrackerListResponse>(
            "/v2/tracker/list", hash => new { hash }, cancellationToken);

        var tracker = result?.List?.FirstOrDefault(t => t.Tracker_id == deviceDto.Identifier);
        return tracker is null
            ? throw new InvalidOperationException($"Device not found: {deviceDto.Identifier}")
            : tracker.Value.MapToDeviceVm(deviceDto);
    }

    /// <summary>
    /// Retrieves multiple devices asynchronously based on the provided device DTOs.
    /// </summary>
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var result = await PostNavixyAsync<TrackerListResponse>(
            "/v2/tracker/list", hash => new { hash }, cancellationToken);

        if (result?.List is null || !result.List.Any())
        {
            return [];
        }

        var devicesDictionary = devices.ToDictionary(device => device.Identifier, device => device);
        return result.List
            .Where(t => devicesDictionary.ContainsKey((int)t.Tracker_id))
            .MapToDeviceVm(devicesDictionary);
    }

    /// <summary>
    /// Retrieves all devices asynchronously.
    /// </summary>
    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var result = await PostNavixyAsync<TrackerListResponse>(
            "/v2/tracker/list", hash => new { hash }, cancellationToken);

        return result?.List is null ? [] : result.List.MapToDeviceVm().Distinct();
    }
}
