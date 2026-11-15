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
using TrackHub.Router.Infrastructure.RedGps.Mappers;

namespace TrackHub.Router.Infrastructure.RedGps;

public sealed class DeviceReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
    : RedGpsReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IExternalDeviceReader
{
    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var vehicle = (await ListVehiclesAsync(cancellationToken)).FirstOrDefault(v => v.Idgps == deviceDto.Serial);
        return vehicle is null
            ? throw new InvalidOperationException($"Device not found: {deviceDto.Serial}")
            : vehicle.MapToDeviceVm(deviceDto);
    }

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesDictionary = devices.ToDeviceLookup(device => device.Serial);
        return (await ListVehiclesAsync(cancellationToken)).MapToDeviceVm(devicesDictionary).ToList();
    }

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
        => (await ListVehiclesAsync(cancellationToken)).Select(v => v.MapToDeviceVm()).Distinct().ToList();

    // A vehicle without an installed GPS has nothing the Router could later query positions for.
    private async Task<IEnumerable<Vehicle>> ListVehiclesAsync(CancellationToken cancellationToken)
        => (await CallAsync<Vehicle>("vehicleGetAll", [], cancellationToken)).Where(v => !string.IsNullOrEmpty(v.Idgps));
}
