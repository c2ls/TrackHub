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

using TrackHub.TripManagement.Application.Integration.Commands.ImportTrips;
using TrackHub.TripManagement.Application.Integration.Commands.UpdateTripStatus;
using TrackHub.TripManagement.Application.TripEvents.Commands.ProcessTripPositions;

namespace TrackHub.TripManagement.Web.GraphQL.Mutation;

/// <summary>
/// Service-client surface: the Router/SyncWorker position feed and the partner import/status API.
/// Kept apart from the user-facing trip mutations because the position feed lives on its own
/// <c>TripTracking</c> resource — a compromised Router identity must not be able to read or mutate
/// trips (spec 11 §18.15).
/// </summary>
public partial class Mutation
{
    public async Task<TripProcessingResultVm> ProcessTripPositions([Service] ISender sender, ProcessTripPositionsCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<IReadOnlyCollection<TripImportResultVm>> ImportTrips([Service] ISender sender, ImportTripsCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<IReadOnlyCollection<TripImportResultVm>> UpdateTripStatus([Service] ISender sender, UpdateTripStatusCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);
}
