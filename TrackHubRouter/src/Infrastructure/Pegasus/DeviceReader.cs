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
using TrackHub.Router.Infrastructure.Pegasus.Mappers;

namespace TrackHub.Router.Infrastructure.Pegasus;

public sealed class DeviceReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
    : PegasusReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IExternalDeviceReader
{
    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var vehicle = await GetAsync<Vehicle>($"vehicles/{deviceDto.Identifier}", cancellationToken);
        return vehicle is not null && vehicle.HasDevice
            ? DeviceMapper.Instance.Map(vehicle, deviceDto)
            : throw new InvalidOperationException($"Pegasus vehicle {deviceDto.Identifier} not found or has no device assigned.");
    }

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesDictionary = devices.ToDeviceLookup(device => device.Serial);
        var vehicles = await GetAllVehiclesAsync<Vehicle>(null, cancellationToken);
        return DeviceMapper.Instance.Map(vehicles, devicesDictionary).Distinct();
    }

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var vehicles = await GetAllVehiclesAsync<Vehicle>(null, cancellationToken);
        return DeviceMapper.Instance.Map(vehicles).Distinct();
    }
}
