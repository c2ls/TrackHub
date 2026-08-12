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

using Geotab.Checkmate.ObjectModel;
using TrackHub.Router.Infrastructure.Geotab.Mappers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Infrastructure.Geotab;

public sealed class PositionReader(IProviderSessionStore sessionStore)
    : GeotabReaderBase(sessionStore), IPositionReader
{
    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var search = new DeviceStatusInfoSearch
        {
            DeviceSearch = new DeviceSearch
            {
                Id = Id.Create(deviceDto.Identifier)
            }
        };
        var position = await GeotabApi!.CallAsync<DeviceStatusInfo>("Get", typeof(DeviceStatusInfo), search, cancellationToken);
        PersistSession();
        return position!.MapToPositionVm(deviceDto);
    }

    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var search = new DeviceStatusInfoSearch
        {
            DeviceSearch = new DeviceSearch
            {
                DeviceIds = devices.Select(device => Id.Create(device.Identifier))
            }
        };
        var positions = await GeotabApi!.CallAsync<IEnumerable<DeviceStatusInfo>>("Get", typeof(DeviceStatusInfo), search, cancellationToken);
        PersistSession();
        if (positions is null)
        {
            return [];
        }
        var devicesDictionary = devices.ToDictionary(device => device.Name, device => device);
        return positions.MapToPositionVm(devicesDictionary).Distinct();
    }

    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var logRecordSearch = new LogRecordSearch
        {
            DeviceSearch = new DeviceSearch(Id.Create(deviceDto.Identifier)),
            // Geotab SDK search bounds are UTC DateTime; convert at the SDK boundary.
            FromDate = from.UtcDateTime,
            ToDate = to.UtcDateTime
        };
        var positions = await GeotabApi!.CallAsync<IEnumerable<LogRecord>>("Get", typeof(LogRecord), new { search = logRecordSearch }, cancellationToken);
        PersistSession();
        return positions is null ? ([]) : positions.MapToPositionVm(deviceDto);
    }
}
