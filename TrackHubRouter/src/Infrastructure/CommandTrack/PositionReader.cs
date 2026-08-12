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

namespace TrackHub.Router.Infrastructure.CommandTrack;

using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Models;
using TrackHub.Router.Infrastructure.CommandTrack.Mappers;
using Common.Domain.Extensions;
using TrackHub.Router.Domain.Extensions;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Interfaces.Operator;

public sealed class PositionReader(ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore
    ) : CommandTrackReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IPositionReader
{
    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var url = $"DataConnectAPI/api/Device/{deviceDto.Identifier}";
        var position = await HttpClientService.GetAsync<DevicePosition>(url, Header, cancellationToken);
        return position.MapToPositionVm(deviceDto);
    }

    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var url = $"DataConnectAPI/api/Devices?{devices.GetIdsQueryString()}";
        var positions = await HttpClientService.GetAsync<IEnumerable<DevicePosition>>(url, Header, cancellationToken);
        if (positions is null)
        {
            return [];
        }
        var devicesDictionary = devices.ToDictionary(device => device.Name, device => device);
        return positions.MapToPositionVm(devicesDictionary).Distinct();
    }

    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var url = $"DataConnectAPI/api/Position/{deviceDto.Name}/{from.ToIso8601String()}/{to.ToIso8601String()}";
        var positions = await HttpClientService.GetAsync<IEnumerable<Position>>(url, Header, cancellationToken);
        return positions is null ? ([]) : positions.MapToPositionVm(deviceDto);
    }
}
