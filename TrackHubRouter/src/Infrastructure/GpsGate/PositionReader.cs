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

using TrackHub.Router.Infrastructure.GpsGate.Mappers;
using TrackHub.Router.Domain.Interfaces;

namespace TrackHub.Router.Infrastructure.GpsGate;

// This class represents a reader for GpsGate api - positions.
public sealed class PositionReader(
    ICredentialHttpClientFactory httpClientFactory, 
    IHttpClientService httpClientService) : GpsGateReaderBase(httpClientFactory, httpClientService), IPositionReader
{

    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        // Use device endpoint to retrieve current device which contains last known position
        var url = $"api/v.1/{ApplicationId}/users/{UserId}/devices/{deviceDto.Identifier}";
        var device = await HttpClientService.GetAsync<Device>(url, cancellationToken: cancellationToken);

        // Map Device coordinates to PositionVm using mapper
        return device.MapToPositionVm(deviceDto);
    }

    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        // GpsGate doesn't provide bulk last position API; fetch device endpoint per device sequentially
        var results = new List<PositionVm>();
        foreach (var device in devices)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var position = await GetDevicePositionAsync(device, cancellationToken);
            results.Add(position);
        }
        return results.Distinct();
    }

    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("GpsGate position history API is not implemented yet.");
    }
}
