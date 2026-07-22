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

using TrackHub.TripManagement.Application.Trips.Commands.Assign;
using TrackHub.TripManagement.Application.Trips.Commands.Create;
using TrackHub.TripManagement.Application.Trips.Commands.Delete;
using TrackHub.TripManagement.Application.Trips.Commands.Lifecycle;
using TrackHub.TripManagement.Application.Trips.Commands.PlanRoute;
using TrackHub.TripManagement.Application.Trips.Commands.Update;

namespace TrackHub.TripManagement.Web.GraphQL.Mutation;

/// <summary>Trip CRUD, assignment, route planning and the lifecycle transitions.</summary>
public partial class Mutation
{
    public async Task<TripVm> CreateTrip([Service] ISender sender, CreateTripCommand command)
        => await sender.Send(command);

    public async Task<bool> UpdateTrip([Service] ISender sender, UpdateTripCommand command)
    {
        await sender.Send(command);
        return true;
    }

    // Delete mutations return the deleted identifier, not a boolean (rules.md naming).
    public async Task<Guid> DeleteTrip([Service] ISender sender, Guid id)
    {
        await sender.Send(new DeleteTripCommand(id));
        return id;
    }

    public async Task<TripAssignmentVm> AssignTrip([Service] ISender sender, AssignTripCommand command)
        => await sender.Send(command);

    public async Task<RoutePlanVm> PlanTripRoute([Service] ISender sender, PlanTripRouteCommand command)
        => await sender.Send(command);

    public async Task<bool> StartTrip([Service] ISender sender, Guid id)
    {
        await sender.Send(new StartTripCommand(id));
        return true;
    }

    public async Task<bool> PauseTrip([Service] ISender sender, Guid id)
    {
        await sender.Send(new PauseTripCommand(id));
        return true;
    }

    public async Task<bool> ResumeTrip([Service] ISender sender, Guid id)
    {
        await sender.Send(new ResumeTripCommand(id));
        return true;
    }

    public async Task<bool> CompleteTrip([Service] ISender sender, CompleteTripCommand command)
    {
        await sender.Send(command);
        return true;
    }

    public async Task<bool> CancelTrip([Service] ISender sender, CancelTripCommand command)
    {
        await sender.Send(command);
        return true;
    }

    public async Task<bool> AbortTrip([Service] ISender sender, AbortTripCommand command)
    {
        await sender.Send(command);
        return true;
    }
}
