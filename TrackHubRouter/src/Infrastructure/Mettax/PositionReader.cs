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
using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Infrastructure.Mettax.Mappers;

namespace TrackHub.Router.Infrastructure.Mettax;

public sealed class PositionReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    IProviderSessionStore sessionStore)
    : MettaxReaderBase(httpClientFactory, httpClientService, sessionStore), IPositionReader
{
    internal const string ShadowUrl = "v2/openapi/device/shadow/deviceIds";
    internal const string HistoryUrl = "v2/openapi/gps/history";
    internal const int HistoryPageSize = 100;
    internal static readonly TimeSpan MaxHistoryWindow = TimeSpan.FromDays(30) - TimeSpan.FromSeconds(1);

    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
        => (await GetDevicePositionAsync([deviceDto], cancellationToken)).Cast<PositionVm?>().FirstOrDefault()
            ?? throw new InvalidOperationException($"Mettax device {deviceDto.Serial} has no reported position.");

    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var lookup = devices.ToDeviceLookup(device => device.Serial);
        var positions = new List<PositionVm>();
        foreach (var batch in lookup.Keys.Chunk(ProviderBatching.MaxIdsPerRequest))
        {
            var shadows = await PostAsync<IReadOnlyList<DeviceShadow>>(
                ShadowUrl, new { deviceIds = string.Join(',', batch) }, cancellationToken) ?? [];
            foreach (var shadow in shadows)
            {
                if (shadow.DeviceData is { DeviceId: { } id } fix
                    && lookup.TryGetValue(id, out var device)
                    && fix.MapToPositionVm(shadow.Expand?.ReportTime, device) is { } position)
                {
                    positions.Add(position);
                }
            }
        }
        return positions;
    }

    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var positions = new List<PositionVm>();
        foreach (var (start, end) in TimeWindows.Split(from, to, MaxHistoryWindow))
        {
            var fetched = 0L;
            for (var pageIndex = 1; ; pageIndex++)
            {
                var page = await PostAsync<Page<Fix>>(HistoryUrl, new
                {
                    deviceId = deviceDto.Serial,
                    startTime = UtcTimestamps.Format(start),
                    endTime = UtcTimestamps.Format(end),
                    pageIndex,
                    pageSize = HistoryPageSize
                }, cancellationToken);
                var records = page?.Records ?? [];
                positions.AddRange(records
                    .Select(record => record.MapToPositionVm(record.ReportTime, deviceDto))
                    .OfType<PositionVm>()
                    .Where(position => position.DeviceDateTime >= from && position.DeviceDateTime <= to));
                fetched += records.Count;
                if (records.Count == 0 || (page?.Total is { } total ? fetched >= total : records.Count < HistoryPageSize))
                {
                    break;
                }
            }
        }
        return positions.Distinct().OrderBy(position => position.DeviceDateTime).ToList();
    }
}
