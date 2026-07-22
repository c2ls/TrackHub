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

namespace TrackHub.TripManagement.Application.TripStops.Commands.Update;

/// <summary>
/// Edits a stop's plan data.
/// <para>
/// The command is addressed by stop id and carries no trip id, so it used to reach the writer with
/// nothing but the account scope — a dispatcher could edit any stop in the account, including one
/// belonging to another group's trip. The owning trip is resolved under the caller's visibility
/// scope first; an invisible or unknown stop is a 404, not a 403 (non-disclosure).
/// </para>
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.TripManagement)]
public readonly record struct UpdateTripStopCommand(Guid TripStopId, TripStopDto Stop) : IRequest;

public sealed class UpdateTripStopCommandHandler(
    ITripStopWriter writer,
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<UpdateTripStopCommand>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task Handle(UpdateTripStopCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);

        await TripVisibility.ResolveVisibleTripByStopAsync(
            reader, request.TripStopId, caller.AccountId, scopeUserId, cancellationToken);

        await TripVisibility.EnsureGeofenceInAccountAsync(reader, request.Stop.GeofenceId, caller.AccountId, cancellationToken);

        await writer.UpdateStopAsync(request.TripStopId, caller.AccountId, request.Stop, cancellationToken);
    }
}

public sealed class UpdateTripStopValidator : AbstractValidator<UpdateTripStopCommand>
{
    public UpdateTripStopValidator()
    {
        RuleFor(v => v.TripStopId).NotEmpty();
        RuleFor(v => v.Stop).SetValidator(new TripStopDtoValidator());
    }
}
