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
using TrackHub.Router.Infrastructure.Motive.Mappers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Domain.Extensions;
using TrackHub.Router.Domain.Helpers;

namespace TrackHub.Router.Infrastructure.Motive;

public sealed class PositionReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService)
    : MotiveReaderBase(httpClientFactory, httpClientService), IPositionReader
{
    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var positions = await GetDevicePositionAsync([deviceDto], cancellationToken);
        return positions.Cast<PositionVm?>().FirstOrDefault()
            ?? throw new InvalidOperationException($"Device not found: {deviceDto.Serial}");
    }

    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesDictionary = devices.ToDeviceLookup(device => device.Serial);
        var results = new List<PositionVm>();
        foreach (var chunk in devicesDictionary.Keys.Chunk(ProviderBatching.MaxIdsPerRequest))
        {
            var vehicles = await GetAllVehiclesAsync($"v1/vehicle_locations?{VehicleIdsQuery(chunk)}", cancellationToken);
            results.AddRange(vehicles.MapToPositionVm(devicesDictionary));
        }
        return results.Distinct();
    }

    // v2 serves every vehicle (v3 needs a Vehicle Gateway) but filters by whole dates only, so results are trimmed.
    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var positions = new List<PositionVm>();
        foreach (var (start, end) in DateWindows(from, to))
        {
            var url = $"v2/vehicle_locations/{Uri.EscapeDataString(deviceDto.Serial)}?start_date={FormatDate(start)}&end_date={FormatDate(end)}"
                + $"&updated_after={FormatDate(start.AddDays(-1))}";

            // The endpoint documents no paging, but every Motive GET defaults to 25 results: page while it says so.
            for (var page = 1; ; page++)
            {
                var response = await HttpClientService.GetAsync<VehicleLocationsResponse>(
                    $"{url}&per_page={PageSize}&page_no={page}", cancellationToken: cancellationToken);
                if (response?.VehicleLocations is null)
                {
                    throw new InvalidOperationException($"Motive answered without a location list for vehicle {deviceDto.Serial}.");
                }

                var locations = response.VehicleLocations.Select(entry => entry.VehicleLocation).ToList();
                positions.AddRange(locations
                    .Where(location => location.LocatedAt >= from && location.LocatedAt <= to)
                    .Select(location => location.MapToPositionVm(deviceDto)));

                if (locations.Count == 0 || response.Pagination is not { } pagination || pagination.PageNo * pagination.PerPage >= pagination.Total)
                {
                    break;
                }
            }
        }
        return positions;
    }

    internal static IEnumerable<(DateOnly Start, DateOnly End)> DateWindows(DateTimeOffset from, DateTimeOffset to)
    {
        var start = DateOnly.FromDateTime(from.UtcDateTime);
        var last = DateOnly.FromDateTime(to.UtcDateTime);
        while (start <= last)
        {
            var end = start.AddMonths(3).AddDays(-1);
            if (end > last)
            {
                end = last;
            }
            yield return (start, end);
            start = end.AddDays(1);
        }
    }

    private static string FormatDate(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
