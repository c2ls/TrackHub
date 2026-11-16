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

public sealed class DeviceReader(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
    : TrustTrackReaderBase(httpClientFactory, httpClientService), IExternalDeviceReader
{
    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var trackedObject = await GetAsync<TrackedObject>(
            $"objects-last-coordinate/{Uri.EscapeDataString(deviceDto.Serial)}", "version=3", cancellationToken);
        return trackedObject is { Id.Length: > 0 }
            ? DeviceMapper.Instance.Map(trackedObject, deviceDto)
            : throw new InvalidOperationException($"TrustTrack object not found: {deviceDto.Serial}");
    }

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
        => DeviceMapper.Instance.Map(await ListObjectsAsync(cancellationToken), devices.ToDeviceLookup(device => device.Serial)).Distinct();

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
        => DeviceMapper.Instance.Map(await ListObjectsAsync(cancellationToken)).Distinct();

    private Task<IReadOnlyList<TrackedObject>> ListObjectsAsync(CancellationToken cancellationToken)
        => GetAsync<IReadOnlyList<TrackedObject>>("objects", "version=1", cancellationToken);
}
