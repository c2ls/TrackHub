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

using TrackHub.Router.Infrastructure.Webfleet.Mappers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Domain.Extensions;

namespace TrackHub.Router.Infrastructure.Webfleet;

public sealed class DeviceReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService)
    : WebfleetReaderBase(httpClientFactory, httpClientService), IExternalDeviceReader
{
    internal const string ObjectReportAction = "showObjectReportExtern";

    public async Task<DeviceVm> GetDeviceAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var reports = await GetListAsync<ObjectReport>(
            ObjectReportAction, $"&objectno={Uri.EscapeDataString(deviceDto.Serial)}", cancellationToken);

        return reports.FirstOrDefault(r => r.Objectno == deviceDto.Serial) is { } report
            ? DeviceMapper.Instance.Map(report, deviceDto)
            : throw new InvalidOperationException($"Device not found: {deviceDto.Serial}");
    }

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesDictionary = devices.ToDeviceLookup(device => device.Serial);
        var reports = await GetListAsync<ObjectReport>(ObjectReportAction, string.Empty, cancellationToken);
        return [.. DeviceMapper.Instance.Map(reports, devicesDictionary)];
    }

    public async Task<IEnumerable<DeviceVm>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var reports = await GetListAsync<ObjectReport>(ObjectReportAction, string.Empty, cancellationToken);
        return [.. DeviceMapper.Instance.Map(reports.Where(r => r.Deleted != true)).Distinct()];
    }
}
