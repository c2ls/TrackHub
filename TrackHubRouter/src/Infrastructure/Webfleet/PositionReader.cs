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

using System.Globalization;
using TrackHub.Router.Infrastructure.Webfleet.Mappers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Domain.Extensions;
using TrackHub.Router.Domain.Helpers;

namespace TrackHub.Router.Infrastructure.Webfleet;

public sealed class PositionReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService)
    : WebfleetReaderBase(httpClientFactory, httpClientService), IPositionReader
{
    internal const string TracksAction = "showTracks";

    // showTracks rejects periods longer than 2 days.
    internal static readonly TimeSpan MaxTrackWindow = TimeSpan.FromDays(2);

    // showTracks allows 10 requests a minute, and a quota breach blocks the account until a quiet interval passes.
    internal const int TrackRequestsPerMinute = 10;
    internal static TimeSpan TrackRequestSpacing { get; set; } = TimeSpan.FromSeconds(6);

    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var reports = await GetListAsync<ObjectReport>(
            DeviceReader.ObjectReportAction, $"&objectno={Uri.EscapeDataString(deviceDto.Serial)}", cancellationToken);

        return reports.FirstOrDefault(r => r.Objectno == deviceDto.Serial && r.HasPosition()) is { } report
            ? report.MapToPositionVm(deviceDto)
            : throw new InvalidOperationException($"Device not found or without a position: {deviceDto.Serial}");
    }

    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesDictionary = devices.ToDeviceLookup(device => device.Serial);
        var reports = await GetListAsync<ObjectReport>(DeviceReader.ObjectReportAction, string.Empty, cancellationToken);
        return [.. reports.MapToPositionVm(devicesDictionary).Distinct()];
    }

    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var positions = new List<PositionVm>();
        var windows = SplitRange(from, to).ToList();
        foreach (var (windowFrom, windowTo) in windows)
        {
            if (windows.Count > TrackRequestsPerMinute && windowFrom != windows[0].From)
            {
                await Task.Delay(TrackRequestSpacing, cancellationToken);
            }

            var points = await GetListAsync<TrackPoint>(
                TracksAction,
                $"&objectno={Uri.EscapeDataString(deviceDto.Serial)}&range_pattern=ud"
                    + $"&rangefrom_string={FormatRangeBound(windowFrom)}&rangeto_string={FormatRangeBound(windowTo)}",
                cancellationToken);
            positions.AddRange(points.Where(point => point.HasPosition()).Select(point => point.MapToPositionVm(deviceDto)));
        }

        return [.. positions.Where(p => p.DeviceDateTime >= from && p.DeviceDateTime <= to).Distinct()];
    }

    internal static IEnumerable<(DateTimeOffset From, DateTimeOffset To)> SplitRange(DateTimeOffset from, DateTimeOffset to)
        => TimeWindows.Split(from, to, MaxTrackWindow, contiguous: true);

    // With useISO8601=true range parameters are read as ISO 8601 instead of the lang-specific dd/MM/yyyy.
    internal static string FormatRangeBound(DateTimeOffset value)
        => value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
}
