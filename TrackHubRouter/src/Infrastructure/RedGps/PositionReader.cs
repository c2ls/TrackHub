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
using TrackHub.Router.Infrastructure.RedGps.Mappers;

namespace TrackHub.Router.Infrastructure.RedGps;

public sealed class PositionReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
    : RedGpsReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IPositionReader
{
    // history/get allows 15 days per device; the window is padded on both sides by RequestZonePadding.
    internal static readonly TimeSpan MaxHistoryWindow =
        TimeSpan.FromDays(15) - 2 * PositionMapper.RequestZonePadding - TimeSpan.FromSeconds(1);

    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var unit = (await GetDataAsync(cancellationToken)).FirstOrDefault(u => u.GpsIdentif == deviceDto.Serial)
            ?? throw new InvalidOperationException($"Device not found: {deviceDto.Serial}");
        return GpsFix.IsValid(unit.Latitude, unit.Longitude) && PositionMapper.ParseUtcReportDate(unit.ReportDate) is { } deviceTime
            ? unit.MapToPositionVm(deviceTime, deviceDto)
            : throw new InvalidOperationException($"Device has no position: {deviceDto.Serial}");
    }

    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesDictionary = devices.ToDeviceLookup(device => device.Serial);
        return (await GetDataAsync(cancellationToken)).MapToPositionVm(devicesDictionary).Distinct().ToList();
    }

    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var positions = new List<PositionVm>();
        foreach (var (start, end) in SplitRange(from, to))
        {
            var (fechaIni, fechaFin) = PositionMapper.RequestWindow(start, end);
            var points = await CallAsync<HistoryPoint>("history/get",
            [
                new("equipo", deviceDto.Serial),
                new("fechaIni", fechaIni),
                new("fechaFin", fechaFin),
                new("format", "DateTime")
            ], cancellationToken);
            positions.AddRange(points.MapToPositionVm(from, to, deviceDto));
        }
        return positions.Distinct().ToList();
    }

    internal static IEnumerable<(DateTimeOffset Start, DateTimeOffset End)> SplitRange(DateTimeOffset from, DateTimeOffset to)
        => TimeWindows.Split(from, to, MaxHistoryWindow);

    private Task<List<UnitData>> GetDataAsync(CancellationToken cancellationToken)
        => CallAsync<UnitData>("getdata", [new("UseUTCDate", "1"), new("sensores", "0")], cancellationToken);
}
