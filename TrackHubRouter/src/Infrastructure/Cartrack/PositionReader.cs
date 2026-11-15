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
using TrackHub.Router.Infrastructure.Cartrack.Mappers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Domain.Extensions;
using TrackHub.Router.Domain.Helpers;

namespace TrackHub.Router.Infrastructure.Cartrack;

public sealed class PositionReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService)
    : CartrackReaderBase(httpClientFactory, httpClientService), IPositionReader
{
    // The docs give the input format without a zone and read other naive inputs in the account's zone, so each
    // request is widened by the largest UTC offset on both sides and events are filtered by their own instant.
    internal static readonly TimeSpan RequestZonePadding = TimeWindows.LargestUtcOffset;

    internal static readonly TimeSpan MaxEventWindow = TimeSpan.FromDays(31) - 2 * RequestZonePadding - TimeSpan.FromSeconds(1);

    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var response = await GetPageAsync<VehicleStatus>(
            $"vehicles/status?filter[registration]={Uri.EscapeDataString(deviceDto.Serial)}", cancellationToken);
        var lookup = new Dictionary<string, DeviceTransporterVm> { [deviceDto.Serial] = deviceDto };
        return response.Data!.MapToPositionVm(lookup).Cast<PositionVm?>().FirstOrDefault()
            ?? throw new InvalidOperationException($"Device not found: {deviceDto.Serial}");
    }

    // The status endpoint returns the whole fleet in one call and is limited to 60 calls a minute.
    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesDictionary = devices.ToDeviceLookup(device => device.Serial);
        var response = await GetPageAsync<VehicleStatus>("vehicles/status", cancellationToken);
        return response.Data!.MapToPositionVm(devicesDictionary).Distinct().ToList();
    }

    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var positions = new List<PositionVm>();
        foreach (var (start, end) in EventWindows(from, to))
        {
            var url = $"vehicles/{Uri.EscapeDataString(deviceDto.Serial)}/events"
                + $"?start_timestamp={FormatTimestamp(start - RequestZonePadding)}&end_timestamp={FormatTimestamp(end + RequestZonePadding)}";
            var events = await GetAllPagesAsync<VehicleEvent>(url, cancellationToken);
            positions.AddRange(events
                .Where(vehicleEvent => vehicleEvent.HasFix() && vehicleEvent.EventTs >= from && vehicleEvent.EventTs <= to)
                .Select(vehicleEvent => vehicleEvent.MapToPositionVm(deviceDto)));
        }
        return positions.Distinct().ToList();
    }

    internal static IEnumerable<(DateTimeOffset Start, DateTimeOffset End)> EventWindows(DateTimeOffset from, DateTimeOffset to)
        => TimeWindows.Split(from, to, MaxEventWindow);

    internal static string FormatTimestamp(DateTimeOffset value)
        => Uri.EscapeDataString(value.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
}
