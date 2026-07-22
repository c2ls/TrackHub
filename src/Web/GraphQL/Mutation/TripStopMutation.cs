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

using TrackHub.TripManagement.Application.TripStops.Commands.Add;
using TrackHub.TripManagement.Application.TripStops.Commands.Progress;
using TrackHub.TripManagement.Application.TripStops.Commands.Remove;
using TrackHub.TripManagement.Application.TripStops.Commands.Reorder;
using TrackHub.TripManagement.Application.TripStops.Commands.Update;

namespace TrackHub.TripManagement.Web.GraphQL.Mutation;

/// <summary>Stop structure plus the dispatcher-side progress overrides.</summary>
public partial class Mutation
{
    public async Task<TripStopVm> AddTripStop([Service] ISender sender, AddTripStopCommand command)
        => await sender.Send(command);

    public async Task<bool> UpdateTripStop([Service] ISender sender, UpdateTripStopCommand command)
    {
        await sender.Send(command);
        return true;
    }

    public async Task<Guid> RemoveTripStop([Service] ISender sender, Guid id)
    {
        await sender.Send(new RemoveTripStopCommand(id));
        return id;
    }

    public async Task<bool> ReorderTripStops([Service] ISender sender, ReorderTripStopsCommand command)
    {
        await sender.Send(command);
        return true;
    }

    // The three idempotent progress commands: a duplicate clientEventId returns false (nothing was
    // written) rather than an error, so an offline outbox can drain without special-casing retries.
    public async Task<bool> RecordStopArrival([Service] ISender sender, RecordStopArrivalCommand command)
        => await sender.Send(command);

    public async Task<bool> RecordStopDeparture([Service] ISender sender, RecordStopDepartureCommand command)
        => await sender.Send(command);

    public async Task<bool> SkipStop([Service] ISender sender, SkipStopCommand command)
        => await sender.Send(command);
}
