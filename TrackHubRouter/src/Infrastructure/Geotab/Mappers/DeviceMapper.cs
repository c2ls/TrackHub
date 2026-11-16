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
using TrackHub.Router.Domain.Mappers;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Infrastructure.Geotab.Mappers;

internal sealed class DeviceMapper : DeviceMapperBase<Device, int>
{
    public static DeviceMapper Instance { get; } = new();

    protected override int Key(Device device) => (int)device.Id!.GetValue();

    protected override string Serial(Device device) => device.SerialNumber ?? string.Empty;

    protected override string Name(Device device) => device.Name ?? string.Empty;

    protected override int Identifier(Device device, DeviceTransporterVm? assignment) => (int)device.Id!.GetValue();
}
