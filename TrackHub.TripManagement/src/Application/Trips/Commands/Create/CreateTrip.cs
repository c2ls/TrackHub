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

namespace TrackHub.TripManagement.Application.Trips.Commands.Create;

/// <summary>
/// Creates a trip in the CALLER's account. <see cref="TripDto"/> deliberately carries no
/// <c>AccountId</c>: the tenant is resolved from the token, never accepted from the wire
/// (acceptance 1).
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Write)]
[RequireFeature(FeatureKeys.TripManagement)]
// Enforcement: the handler derives the caller's own account and passes it to the reader/writer,
// which filters every row on it (TripVisibility is the single visibility resolver - spec 11).
[AccountScopeEnforcedInHandler]
public readonly record struct CreateTripCommand(TripDto Trip) : IRequest<TripVm>;

public sealed class CreateTripCommandHandler(
    ITripWriter writer,
    ITripReader reader,
    IUserReader userReader,
    IUser user,
    IManagerValidationClient managerValidationClient,
    IServiceOrderValidator serviceOrderValidator,
    ITransporterTollClassStore tollClassStore) : IRequestHandler<CreateTripCommand, TripVm>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<TripVm> Handle(CreateTripCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);

        // Group visibility AND account, through the single visibility source (acceptance 3, 4).
        await TripVisibility.EnsureTransporterVisibleAsync(reader, user, caller.AccountId, UserId, request.Trip.TransporterId, cancellationToken);

        // Every referenced parent is validated against the trip's account at write time
        // (acceptance 2). A driver qualifies only through Manager's assignment rules.
        if (request.Trip.DriverId is { } driverId)
        {
            var assignable = await managerValidationClient.ValidateDriverAssignmentAsync(
                driverId, "Transporter", request.Trip.TransporterId, cancellationToken);
            if (!assignable)
                throw new ForbiddenAccessException(Resources.Trips, Actions.Write, TripErrorCodes.DriverNotAssignable);
        }

        // Spec 11 §5 lists ServiceOrderId among the cross-account references validated at write
        // time. Spec 12 owns service orders, so this goes through a port whose default accepts any
        // reference — the call site is correct today and starts enforcing the moment spec 12
        // registers a real implementation (see IServiceOrderValidator).
        await TripVisibility.EnsureServiceOrderInAccountAsync(
            serviceOrderValidator, request.Trip.ServiceOrderId, caller.AccountId, cancellationToken);

        var duplicate = await TripLookup.FindByCodeAsync(reader, caller.AccountId, request.Trip.Code, cancellationToken);
        if (duplicate is not null)
            throw ConflictException.WithCode(TripErrorCodes.DuplicateTripCode);

        if (!string.IsNullOrWhiteSpace(request.Trip.ExternalReference))
        {
            var duplicateReference = await TripLookup.FindByExternalReferenceAsync(reader, caller.AccountId, request.Trip.ExternalReference, cancellationToken);
            if (duplicateReference is not null)
                throw ConflictException.WithCode(TripErrorCodes.DuplicateExternalReference);
        }

        var trip = request.Trip;
        if (string.IsNullOrWhiteSpace(trip.TollVehicleClass))
        {
            var resolved = await tollClassStore.ResolveClassAsync(caller.AccountId, trip.TransporterId, cancellationToken);
            trip = trip with { TollVehicleClass = resolved };
        }

        // No TripEvent row is written here on purpose: TripCreated is a DOMAIN event raised by the
        // writer. A TripEvent row would make the trip undeletable the instant it was created,
        // because delete is permitted only for a Created trip with no events (acceptance 16).
        return await writer.CreateTripAsync(trip, caller.AccountId, cancellationToken);
    }
}

public sealed class CreateTripValidator : AbstractValidator<CreateTripCommand>
{
    public CreateTripValidator()
        => RuleFor(v => v.Trip).SetValidator(new TripDtoValidator());
}
