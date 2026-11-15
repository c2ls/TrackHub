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
using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Infrastructure.Plaspy.Mappers;

namespace TrackHub.Router.Infrastructure.Plaspy;

public sealed class PositionReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
    : PlaspyReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IPositionReader
{
    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
        => await ReadLatestAsync(deviceDto, cancellationToken)
            ?? throw new InvalidOperationException($"Plaspy device {deviceDto.Serial} has no reported position.");

    // Plaspy has no bulk position endpoint, so each device costs one request.
    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
        => await ProviderConcurrency.ReadEachDeviceAsync(
            devices.ToDeviceLookup(device => device.Serial).Values,
            async (device, token) => await ReadLatestAsync(device, token) ?? default,
            cancellationToken);

    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var result = await PostAsync<LocationsResponse>(
            $"api/devices/{Uri.EscapeDataString(deviceDto.Serial)}/locations",
            new { from = PositionMapper.FormatUtc(from), to = PositionMapper.FormatUtc(to) },
            cancellationToken);

        return (result.Locations ?? [])
            .Select(location => location.MapToPositionVm(deviceDto))
            .Where(position => position.DeviceDateTime >= from && position.DeviceDateTime <= to)
            .ToList();
    }

    private async Task<PositionVm?> ReadLatestAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var result = await GetAsync<LastLocationResponse>(
            $"api/devices/{Uri.EscapeDataString(deviceDto.Serial)}/lastLocation", cancellationToken);
        return result.LastLocation?.MapToPositionVm(deviceDto);
    }
}
