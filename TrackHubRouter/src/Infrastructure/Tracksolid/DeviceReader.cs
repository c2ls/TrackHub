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
using TrackHub.Router.Infrastructure.Tracksolid.Mappers;

namespace TrackHub.Router.Infrastructure.Tracksolid;

public sealed class DeviceReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
    : TracksolidReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IExternalDeviceReader
{
    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var device = (await ListDevicesAsync(cancellationToken)).FirstOrDefault(d => d.Imei == deviceDto.Serial);
        return device is null
            ? throw new InvalidOperationException($"Device not found: {deviceDto.Serial}")
            : DeviceMapper.Instance.Map(device, deviceDto);
    }

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesDictionary = devices.ToDeviceLookup(device => device.Serial);
        return DeviceMapper.Instance.Map(await ListDevicesAsync(cancellationToken), devicesDictionary).ToList();
    }

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
        => DeviceMapper.Instance.Map(await ListDevicesAsync(cancellationToken)).Distinct().ToList();

    private async Task<IEnumerable<DeviceRecord>> ListDevicesAsync(CancellationToken cancellationToken)
        => await CallAsync<List<DeviceRecord>>("jimi.user.device.list", [new("target", Account)], cancellationToken) ?? [];
}
