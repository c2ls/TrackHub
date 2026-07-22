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

namespace TrackHub.TripManagement.Application.TripStops.Commands.Remove;

/// <summary>
/// Removes a stop. Rejected by the writer once the stop is <c>Arrived</c>/<c>Departed</c> —
/// a stop that happened is part of the record and cannot be erased.
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Delete)]
[RequireFeature(FeatureKeys.TripManagement)]
public readonly record struct RemoveTripStopCommand(Guid TripStopId) : IRequest;

public sealed class RemoveTripStopCommandHandler(
    ITripStopWriter writer,
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<RemoveTripStopCommand>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task Handle(RemoveTripStopCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);

        // Addressed by stop id alone: without this the account was the only boundary, so a
        // dispatcher could delete a stop out of another group's trip.
        await TripVisibility.ResolveVisibleTripByStopAsync(
            reader, request.TripStopId, caller.AccountId, scopeUserId, cancellationToken);

        await writer.RemoveStopAsync(request.TripStopId, caller.AccountId, cancellationToken);
    }
}

public sealed class RemoveTripStopValidator : AbstractValidator<RemoveTripStopCommand>
{
    public RemoveTripStopValidator()
        => RuleFor(v => v.TripStopId).NotEmpty();
}
