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

namespace TrackHub.TripManagement.Application.Trips.Commands.Update;

/// <summary>Rejected once the trip is terminal — a closed trip is history, not a draft.</summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.TripManagement)]
// Enforcement: the handler derives the caller's own account and passes it to the reader/writer,
// which filters every row on it (TripVisibility is the single visibility resolver - spec 11).
[AccountScopeEnforcedInHandler]
public readonly record struct UpdateTripCommand(Guid TripId, TripDto Trip) : IRequest;

public sealed class UpdateTripCommandHandler(
    ITripWriter writer,
    ITripReader reader,
    IUserReader userReader,
    IUser user,
    IManagerValidationClient managerValidationClient,
    IServiceOrderValidator serviceOrderValidator) : IRequestHandler<UpdateTripCommand>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task Handle(UpdateTripCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);
        var trip = await reader.GetTripAsync(request.TripId, caller.AccountId, scopeUserId, cancellationToken);

        if (TripStatuses.IsTerminal(trip.Status))
            throw TripValidationFailure.Create(nameof(UpdateTripCommand.TripId), TripErrorCodes.TripAlreadyTerminal);

        await TripVisibility.EnsureTransporterVisibleAsync(reader, user, caller.AccountId, UserId, request.Trip.TransporterId, cancellationToken);

        if (request.Trip.DriverId is { } driverId)
        {
            var assignable = await managerValidationClient.ValidateDriverAssignmentAsync(
                driverId, "Transporter", request.Trip.TransporterId, cancellationToken);
            if (!assignable)
                throw new ForbiddenAccessException(Resources.Trips, Actions.Edit, TripErrorCodes.DriverNotAssignable);
        }

        await TripVisibility.EnsureServiceOrderInAccountAsync(
            serviceOrderValidator, request.Trip.ServiceOrderId, caller.AccountId, cancellationToken);

        if (!string.Equals(trip.Code, request.Trip.Code, StringComparison.OrdinalIgnoreCase))
        {
            var duplicate = await TripLookup.FindByCodeAsync(reader, caller.AccountId, request.Trip.Code, cancellationToken);
            if (duplicate is not null)
                throw ConflictException.WithCode(TripErrorCodes.DuplicateTripCode);
        }

        await writer.UpdateTripAsync(request.TripId, request.Trip, caller.AccountId, cancellationToken);
    }
}

public sealed class UpdateTripValidator : AbstractValidator<UpdateTripCommand>
{
    public UpdateTripValidator()
    {
        RuleFor(v => v.TripId).NotEmpty();
        RuleFor(v => v.Trip).SetValidator(new TripDtoValidator());
    }
}
