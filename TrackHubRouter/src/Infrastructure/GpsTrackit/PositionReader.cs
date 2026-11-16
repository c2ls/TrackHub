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

using Common.Domain.Helpers;
using TrackHub.Router.Domain.Extensions;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Infrastructure.GpsTrackit.Mappers;

namespace TrackHub.Router.Infrastructure.GpsTrackit;

public sealed class PositionReader(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
    : GpsTrackitReaderBase(httpClientFactory, httpClientService), IPositionReader
{
    internal const string ReadingSearchUrl = "reading/search";
    internal const int ReadingPageSize = 500;

    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
        => (await GetDevicePositionAsync([deviceDto], cancellationToken)).Cast<PositionVm?>().FirstOrDefault()
            ?? throw new InvalidOperationException($"GPS Trackit unit {deviceDto.Serial} has no reported position.");

    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var lookup = devices.ToDeviceLookup(device => device.Serial);
        return (await SearchUnitsAsync(cancellationToken))
            .Select(unit => unit.Serial is { } serial && lookup.TryGetValue(serial, out var device)
                ? unit.LastReading?.MapToPositionVm(device)
                : null)
            .OfType<PositionVm>()
            .ToList();
    }

    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var readings = await SearchAllAsync<Reading>(ReadingSearchUrl, offset => new
        {
            deviceId = deviceDto.Serial,
            dateFrom = UtcTimestamps.Format(from, UtcTimestamps.Iso8601Milliseconds),
            dateTo = UtcTimestamps.Format(to, UtcTimestamps.Iso8601Milliseconds),
            limit = ReadingPageSize,
            offset
        }, cancellationToken);
        return readings
            .Select(reading => reading.MapToPositionVm(deviceDto))
            .OfType<PositionVm>()
            .Where(position => position.DeviceDateTime >= from && position.DeviceDateTime <= to)
            .Distinct()
            .OrderBy(position => position.DeviceDateTime)
            .ToList();
    }
}
