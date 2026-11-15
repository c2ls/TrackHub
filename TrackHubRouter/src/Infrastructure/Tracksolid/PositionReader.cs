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
using TrackHub.Router.Infrastructure.Tracksolid.Mappers;

namespace TrackHub.Router.Infrastructure.Tracksolid;

public sealed class PositionReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
    : TracksolidReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IPositionReader
{
    // jimi.device.track.list accepts at most 7 days per call; timestamps have second precision.
    internal static readonly TimeSpan MaxHistoryWindow = TimeSpan.FromDays(7) - TimeSpan.FromSeconds(1);

    private static readonly TimeSpan EndTimeMargin = TimeSpan.FromSeconds(5);

    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var locations = await CallAsync<List<LocationRecord>>(
            "jimi.device.location.get", [new("imeis", deviceDto.Serial)], cancellationToken);

        var location = locations?.FirstOrDefault(l => l.Imei == deviceDto.Serial)
            ?? throw new InvalidOperationException($"Device not found: {deviceDto.Serial}");
        return GpsFix.IsValid(location.Lat, location.Lng) && PositionMapper.ParseUtc(location.GpsTime) is { } deviceTime
            ? location.MapToPositionVm(deviceTime, deviceDto)
            : throw new InvalidOperationException($"Device has no position: {deviceDto.Serial}");
    }

    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesDictionary = devices.ToDeviceLookup(device => device.Serial);
        var locations = await CallAsync<List<LocationRecord>>(
            "jimi.user.device.location.list", [new("target", Account)], cancellationToken);

        return locations is null ? [] : locations.MapToPositionVm(devicesDictionary).Distinct().ToList();
    }

    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        // track.list rejects an end_time that is not earlier than the current time.
        var latest = DateTimeOffset.UtcNow - EndTimeMargin;
        if (to > latest)
        {
            to = latest;
        }

        var positions = new List<PositionVm>();
        foreach (var (start, end) in SplitRange(from, to))
        {
            var points = await CallAsync<List<TrackPoint>>("jimi.device.track.list",
            [
                new("imei", deviceDto.Serial),
                new("begin_time", PositionMapper.FormatUtc(start)),
                new("end_time", PositionMapper.FormatUtc(end))
            ], cancellationToken);

            if (points is not null)
            {
                positions.AddRange(points.MapToPositionVm(from, to, deviceDto));
            }
        }
        return positions;
    }

    internal static IEnumerable<(DateTimeOffset Start, DateTimeOffset End)> SplitRange(DateTimeOffset from, DateTimeOffset to)
        => TimeWindows.Split(from, to, MaxHistoryWindow);
}
