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
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Infrastructure.Pegasus.Mappers;

namespace TrackHub.Router.Infrastructure.Pegasus;

public sealed class PositionReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
    : PegasusReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IPositionReader
{
    internal const int MaxEventsPerRequest = 200_000;
    private const string RawdataFields = "lat,lon,al,mph,head,sv,io_ign,dev_dist,dev_ign,temp,valid_position";

    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var vehicle = await GetAsync<VehicleLatest>($"vehicles/{deviceDto.Identifier}?select={LatestSelect}", cancellationToken);
        return vehicle?.Device is { Latest: { Loc: { Valid: not false } location } latest }
            ? latest.MapToPositionVm(location, deviceDto)
            : throw new InvalidOperationException($"Pegasus vehicle {deviceDto.Identifier} has no reported position.");
    }

    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesDictionary = devices.ToDeviceLookup(device => device.Serial);
        var vehicles = await GetAllVehiclesAsync<VehicleLatest>(LatestSelect, cancellationToken);
        return vehicles.MapToPositionVm(devicesDictionary).Distinct();
    }

    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var positions = new List<PositionVm>();
        await ReadRangeAsync(from.ToUnixTimeSeconds(), to.ToUnixTimeSeconds(), deviceDto, positions, cancellationToken);
        return positions
            .Where(position => position.DeviceDateTime >= from && position.DeviceDateTime <= to)
            .Distinct();
    }

    // Rawdata stops at 200,000 events per request, so a window that reaches the cap may be truncated:
    // it is halved and read again. The halves share their boundary second; Distinct drops the overlap.
    private async Task ReadRangeAsync(long fromSeconds, long toSeconds, DeviceTransporterVm deviceDto, List<PositionVm> positions, CancellationToken cancellationToken)
    {
        var url = $"rawdata?vehicles={deviceDto.Identifier}"
            + $"&from={PositionMapper.FormatUtc(DateTimeOffset.FromUnixTimeSeconds(fromSeconds))}"
            + $"&to={PositionMapper.FormatUtc(DateTimeOffset.FromUnixTimeSeconds(toSeconds))}"
            + $"&fields={RawdataFields}&filter=valid_position";
        var events = (await GetAsync<RawdataResponse>(url, cancellationToken))?.Events ?? [];
        if (events.Count < MaxEventsPerRequest)
        {
            positions.AddRange(events.MapToPositionVm(deviceDto));
            return;
        }

        if (toSeconds - fromSeconds <= 1)
        {
            throw new InvalidOperationException($"Pegasus vehicle {deviceDto.Identifier} reported more than {MaxEventsPerRequest} events within one second.");
        }

        var middle = fromSeconds + (toSeconds - fromSeconds) / 2;
        await ReadRangeAsync(fromSeconds, middle, deviceDto, positions, cancellationToken);
        await ReadRangeAsync(middle, toSeconds, deviceDto, positions, cancellationToken);
    }
}
