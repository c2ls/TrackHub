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

namespace TrackHub.Router.Infrastructure.Wialon.Mappers;

internal sealed class DeviceMapper : DeviceMapperBase<Unit, int>
{
    public static DeviceMapper Instance { get; } = new();

    protected override int Key(Unit unit) => (int)unit.Id;

    protected override string Serial(Unit unit) => unit.Uid ?? unit.Id.ToString();

    protected override string Name(Unit unit) => unit.Nm;

    protected override int Identifier(Unit unit, DeviceTransporterVm? assignment) => (int)unit.Id;
}
