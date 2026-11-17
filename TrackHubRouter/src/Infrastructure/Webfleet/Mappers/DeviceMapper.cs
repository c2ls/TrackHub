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

namespace TrackHub.Router.Infrastructure.Webfleet.Mappers;

internal sealed class DeviceMapper : DeviceMapperBase<ObjectReport>
{
    public static DeviceMapper Instance { get; } = new();

    protected override string Serial(ObjectReport report) => report.Objectno;

    protected override string Name(ObjectReport report) => report.Objectname ?? report.Objectno;

    protected override int Identifier(ObjectReport report, DeviceTransporterVm? assignment) => assignment?.Identifier ?? 0;
}
