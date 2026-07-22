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

namespace TrackHub.TripManagement.Application.Trips.Commands.Delete;

/// <summary>
/// Permitted only for a <c>Created</c> trip with no recorded events. Trip history is permanent:
/// anything that has actually happened is cancelled, never deleted (acceptance 16).
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Delete)]
[RequireFeature(FeatureKeys.TripManagement)]
public readonly record struct DeleteTripCommand(Guid TripId) : IRequest;

public sealed class DeleteTripCommandHandler(
    ITripWriter writer,
    ITripReader reader,
    ITripEventWriter tripEventWriter,
    IUserReader userReader,
    IUser user) : IRequestHandler<DeleteTripCommand>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task Handle(DeleteTripCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);
        var trip = await reader.GetTripAsync(request.TripId, caller.AccountId, scopeUserId, cancellationToken);

        if (!string.Equals(trip.Status, TripStatuses.Created, StringComparison.Ordinal))
            throw ConflictException.WithCode(TripErrorCodes.TripHasHistory);

        if (await tripEventWriter.HasEventsAsync(request.TripId, caller.AccountId, cancellationToken))
            throw ConflictException.WithCode(TripErrorCodes.TripHasHistory);

        await writer.DeleteTripAsync(request.TripId, caller.AccountId, cancellationToken);
    }
}

public sealed class DeleteTripValidator : AbstractValidator<DeleteTripCommand>
{
    public DeleteTripValidator()
        => RuleFor(v => v.TripId).NotEmpty();
}
