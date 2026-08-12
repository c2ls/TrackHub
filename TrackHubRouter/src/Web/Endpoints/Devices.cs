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

using TrackHub.Router.Application.Devices.Queries.Get;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Web.Endpoints;

public class Devices : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetDevices);
    }

    public async Task<IEnumerable<DeviceVm>> GetDevices(ISender sender, [AsParameters] GetDevicesQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

}
