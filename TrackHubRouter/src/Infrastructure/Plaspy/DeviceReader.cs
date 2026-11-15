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
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Infrastructure.Plaspy.Mappers;

namespace TrackHub.Router.Infrastructure.Plaspy;

public sealed class DeviceReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
    : PlaspyReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IExternalDeviceReader
{
    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var result = await GetAsync<DeviceResponse>($"api/devices/{Uri.EscapeDataString(deviceDto.Serial)}", cancellationToken);
        return result.Device is { Id.Length: > 0 } device
            ? device.MapToDeviceVm(deviceDto.TransporterId)
            : throw new InvalidOperationException($"Plaspy device not found: {deviceDto.Serial}");
    }

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesDictionary = devices.ToDeviceLookup(device => device.Serial);
        var result = await GetAsync<DevicesResponse>("api/devices", cancellationToken);
        return (result.Devices ?? []).MapToDeviceVm(devicesDictionary).Distinct();
    }

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var result = await GetAsync<DevicesResponse>("api/devices", cancellationToken);
        return (result.Devices ?? []).MapToDeviceVm().Distinct();
    }
}
