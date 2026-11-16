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

using TrackHub.Router.Domain.Mappers;

namespace TrackHub.Router.Infrastructure.Tracksolid.Mappers;

internal sealed class DeviceMapper : DeviceMapperBase<DeviceRecord>
{
    public static DeviceMapper Instance { get; } = new();

    protected override string Serial(DeviceRecord device) => device.Imei;

    protected override string Name(DeviceRecord device) => device.DeviceName ?? device.Imei;

    protected override int Identifier(DeviceRecord device, DeviceTransporterVm? assignment) => assignment?.Identifier ?? 0;
}
