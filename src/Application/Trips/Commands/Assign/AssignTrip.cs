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
using Microsoft.Extensions.Logging;
using TrackHub.TripManagement.Application.Common;

namespace TrackHub.TripManagement.Application.Trips.Commands.Assign;

/// <summary>
/// Assigns a driver. Manager owns the assignment rules — a driver qualifies through an active
/// <c>DriverTransporterAssignment</c> or their <c>DefaultTransporterId</c> — so this never
/// re-implements them locally (spec 11 §7.3).
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.TripManagement)]
// Enforcement: the handler derives the caller's own account and passes it to the reader/writer,
// which filters every row on it (TripVisibility is the single visibility resolver - spec 11).
[AccountScopeEnforcedInHandler]
public readonly record struct AssignTripCommand(Guid TripId, Guid DriverId, Guid? TransporterId) : IRequest<TripAssignmentVm>;

public sealed class AssignTripCommandHandler(
    ITripWriter writer,
    ITripReader reader,
    ITripEventWriter tripEventWriter,
    IManagerValidationClient managerValidationClient,
    IAlertEmitter alertEmitter,
    IUserReader userReader,
    IUser user,
    ILogger<AssignTripCommandHandler> logger) : IRequestHandler<AssignTripCommand, TripAssignmentVm>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<TripAssignmentVm> Handle(AssignTripCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);
        var trip = await reader.GetTripAsync(request.TripId, caller.AccountId, scopeUserId, cancellationToken);

        if (TripStatuses.IsTerminal(trip.Status))
            throw TripValidationFailure.Create(nameof(AssignTripCommand.TripId), TripErrorCodes.TripAlreadyTerminal);

        var transporterId = request.TransporterId ?? trip.TransporterId;
        await TripVisibility.EnsureTransporterVisibleAsync(reader, user, caller.AccountId, UserId, transporterId, cancellationToken);

        var assignable = await managerValidationClient.ValidateDriverAssignmentAsync(
            request.DriverId, "Transporter", transporterId, cancellationToken);
        if (!assignable)
            throw new ForbiddenAccessException(Resources.Trips, Actions.Edit, TripErrorCodes.DriverNotAssignable);

        var assignment = await writer.AssignTripAsync(request.TripId, caller.AccountId, request.DriverId, request.TransporterId, cancellationToken);

        await tripEventWriter.AppendAsync(
            caller.AccountId,
            request.TripId,
            null,
            TripEventTypes.TripAssigned,
            assignment.AssignedAt,
            TripEventSources.Portal,
            null,
            $"trip-assign:{assignment.TripAssignmentId:N}",
            cancellationToken);

        // Best-effort and isolated: a Manager outage must never fail the assignment itself.
        try
        {
            await alertEmitter.EmitAsync(
                TripEventTypes.TripAssigned,
                TripAlertSeverities.Info,
                $"trip-assigned:{request.TripId:N}",
                new TripAlertDto(caller.AccountId, request.TripId, null, trip.Code, transporterId, request.DriverId, null, assignment.AssignedAt, null, null, null, null, null),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to emit TripAssigned alert for trip {TripId}", request.TripId);
        }

        return assignment;
    }
}

public sealed class AssignTripValidator : AbstractValidator<AssignTripCommand>
{
    public AssignTripValidator()
    {
        RuleFor(v => v.TripId).NotEmpty();
        RuleFor(v => v.DriverId).NotEmpty();
        RuleFor(v => v.TransporterId).NotEqual(Guid.Empty).When(v => v.TransporterId.HasValue);
    }
}
