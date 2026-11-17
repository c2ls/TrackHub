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

using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Infrastructure.GpsGate.Mappers;
using TrackHub.Router.Domain.Enumerators;
using TrackHub.Router.Domain.Exceptions;
using TrackHub.Router.Domain.Interfaces;

namespace TrackHub.Router.Infrastructure.GpsGate;

// This class represents a reader for GpsGate api - positions.
public sealed class PositionReader(
    ICredentialHttpClientFactory httpClientFactory, 
    IHttpClientService httpClientService) : GpsGateReaderBase(httpClientFactory, httpClientService), IPositionReader
{

    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
        => await ReadLatestAsync(deviceDto, cancellationToken)
            ?? throw new InvalidOperationException($"GpsGate device {deviceDto.Identifier} has no reported position.");

    private async Task<PositionVm?> ReadLatestAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        // The device record carries its last known position.
        var url = $"api/v.1/{ApplicationId}/users/{UserId}/devices/{deviceDto.Identifier}";
        var device = await HttpClientService.GetAsync<Device>(url, cancellationToken: cancellationToken);
        return device.MapToPositionVm(deviceDto);
    }

    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
        // GpsGate provides no bulk last-position API, so the batch is a bounded fan-out rather than
        // a sequential walk: one call per vehicle in series did not fit the 10-second cycle.
        => await ProviderConcurrency.ReadEachDeviceAsync(devices, async (device, token) => await ReadLatestAsync(device, token) ?? default, cancellationToken);

    public Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
        // GpsGate exposes no usable track-history endpoint. Callers are expected to check
        // ProviderCapabilityCatalog first; this throw is defense in depth so a bypassing call
        // still attributes the limitation to the provider instead of a masked server error.
        => throw new ProviderCapabilityNotSupportedException(Protocol, ProviderCapability.PositionHistory);
}
