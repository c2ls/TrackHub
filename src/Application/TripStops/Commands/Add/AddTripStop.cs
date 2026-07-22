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

using Common.Application.Interfaces;
using TrackHub.TripManagement.Application.Common;

namespace TrackHub.TripManagement.Application.TripStops.Commands.Add;

/// <summary>Appends a stop; the writer re-normalizes sequences server-side.</summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Write)]
[RequireFeature(FeatureKeys.TripManagement)]
public readonly record struct AddTripStopCommand(Guid TripId, TripStopDto Stop) : IRequest<TripStopVm>;

public sealed class AddTripStopCommandHandler(
    ITripStopWriter writer,
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<AddTripStopCommand, TripStopVm>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<TripStopVm> Handle(AddTripStopCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);
        var trip = await reader.GetTripAsync(request.TripId, caller.AccountId, scopeUserId, cancellationToken);

        if (TripStatuses.IsTerminal(trip.Status))
            throw TripValidationFailure.Create(nameof(AddTripStopCommand.TripId), TripErrorCodes.TripAlreadyTerminal);

        // A linked geofence is a cross-account reference and is checked here, not at the arrival
        // snapshot: the snapshot's fallback to a radius buffer is silent, so an unknown or
        // cross-account id would have been accepted and then quietly downgraded to a circle.
        await TripVisibility.EnsureGeofenceInAccountAsync(reader, request.Stop.GeofenceId, caller.AccountId, cancellationToken);

        return await writer.AddStopAsync(request.TripId, caller.AccountId, request.Stop, cancellationToken);
    }
}

public sealed class AddTripStopValidator : AbstractValidator<AddTripStopCommand>
{
    public AddTripStopValidator()
    {
        RuleFor(v => v.TripId).NotEmpty();
        RuleFor(v => v.Stop).SetValidator(new TripStopDtoValidator());
    }
}
