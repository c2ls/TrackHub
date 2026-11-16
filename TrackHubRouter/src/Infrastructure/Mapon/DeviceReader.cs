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
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Infrastructure.Mapon.Mappers;

namespace TrackHub.Router.Infrastructure.Mapon;

public sealed class DeviceReader(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
    : MaponReaderBase(httpClientFactory, httpClientService), IExternalDeviceReader
{
    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var units = await ListUnitsAsync($"&unit_id={Uri.EscapeDataString(deviceDto.Serial)}", cancellationToken);
        return units.FirstOrDefault(unit => unit.Serial == deviceDto.Serial) is { } unit
            ? DeviceMapper.Instance.Map(unit, deviceDto)
            : throw new InvalidOperationException($"Mapon unit not found: {deviceDto.Serial}");
    }

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
        => DeviceMapper.Instance.Map(await ListUnitsAsync(string.Empty, cancellationToken), devices.ToDeviceLookup(device => device.Serial)).Distinct();

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
        => DeviceMapper.Instance.Map(await ListUnitsAsync(string.Empty, cancellationToken)).Distinct();
}
