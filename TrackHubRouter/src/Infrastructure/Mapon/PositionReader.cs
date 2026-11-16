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
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Infrastructure.Mapon.Mappers;

namespace TrackHub.Router.Infrastructure.Mapon;

public sealed class PositionReader(ICredentialHttpClientFactory httpClientFactory, IHttpClientService httpClientService)
    : MaponReaderBase(httpClientFactory, httpClientService), IPositionReader
{
    internal const string RouteListUrl = "route/list.json";
    internal const string PositionIncludes = "&include%5B%5D=ignition&include%5B%5D=altitude";
    internal static readonly TimeSpan MaxRouteWindow = TimeSpan.FromDays(31);

    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
        => (await ReadPositionsAsync($"&unit_id={Uri.EscapeDataString(deviceDto.Serial)}", [deviceDto], cancellationToken))
            .Cast<PositionVm?>().FirstOrDefault()
            ?? throw new InvalidOperationException($"Mapon unit {deviceDto.Serial} has no reported position.");

    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
        => await ReadPositionsAsync(string.Empty, devices, cancellationToken);

    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var positions = new List<PositionVm>();
        foreach (var (start, end) in TimeWindows.Split(from, to, MaxRouteWindow))
        {
            var routes = await GetAsync<RouteList>(RouteListUrl,
                $"&unit_id={Uri.EscapeDataString(deviceDto.Serial)}&from={PositionMapper.FormatUtc(start)}&till={PositionMapper.FormatUtc(end)}&include%5B%5D=decoded_route",
                cancellationToken);
            positions.AddRange((routes.Units ?? [])
                .SelectMany(unit => unit.Routes ?? [])
                .Where(route => route.Type == "route")
                .SelectMany(route => route.DecodedRoute?.Points ?? [])
                .Select(point => point.MapToPositionVm(deviceDto))
                .OfType<PositionVm>()
                .Where(position => position.DeviceDateTime >= from && position.DeviceDateTime <= to));
        }
        return positions.Distinct().OrderBy(position => position.DeviceDateTime).ToList();
    }

    private async Task<IReadOnlyList<PositionVm>> ReadPositionsAsync(string query, IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var lookup = devices.ToDeviceLookup(device => device.Serial);
        var units = await ListUnitsAsync(query + PositionIncludes, cancellationToken);
        return units
            .Select(unit => unit.Serial is { } serial && lookup.TryGetValue(serial, out var device)
                ? unit.MapToPositionVm(device)
                : null)
            .OfType<PositionVm>()
            .ToList();
    }
}
