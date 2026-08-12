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

namespace TrackHub.Router.Infrastructure.GpsGate.Mappers;

internal static class PositionMapper
{

    public static PositionVm MapToPositionVm(this Device device, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            device.Latitude,
            device.Longitude,
            null,
            device.TimeStamp ?? DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            0,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );
}
