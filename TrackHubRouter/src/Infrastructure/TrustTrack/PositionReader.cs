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
using TrackHub.Router.Infrastructure.TrustTrack.Mappers;

namespace TrackHub.Router.Infrastructure.TrustTrack;

public sealed class PositionReader(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
    : TrustTrackReaderBase(httpClientFactory, httpClientService), IPositionReader
{
    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var trackedObject = await GetAsync<TrackedObject>(
            $"objects-last-coordinate/{Uri.EscapeDataString(deviceDto.Serial)}", "version=3", cancellationToken);
        return trackedObject.MapToPositionVm(deviceDto)
            ?? throw new InvalidOperationException($"TrustTrack object {deviceDto.Serial} has no reported position.");
    }

    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var lookup = devices.ToDeviceLookup(device => device.Serial);
        var trackedObjects = await GetAllPagesAsync<TrackedObject>("objects-last-coordinate", "version=3", cancellationToken);
        return trackedObjects
            .Select(trackedObject => trackedObject.Id is { } id && lookup.TryGetValue(id, out var device)
                ? trackedObject.MapToPositionVm(device)
                : null)
            .OfType<PositionVm>()
            .Distinct()
            .ToList();
    }

    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var coordinates = await GetAllPagesAsync<Coordinate>(
            $"objects/{Uri.EscapeDataString(deviceDto.Serial)}/coordinates",
            $"version=3&from_datetime={PositionMapper.FormatUtc(from)}&to_datetime={PositionMapper.FormatUtc(to)}",
            cancellationToken);
        return coordinates
            .Select(coordinate => coordinate.MapToPositionVm(deviceDto))
            .OfType<PositionVm>()
            .Where(position => position.DeviceDateTime >= from && position.DeviceDateTime <= to)
            .Distinct()
            .OrderBy(position => position.DeviceDateTime)
            .ToList();
    }
}
