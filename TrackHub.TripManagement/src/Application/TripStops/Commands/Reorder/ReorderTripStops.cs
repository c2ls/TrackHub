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

namespace TrackHub.TripManagement.Application.TripStops.Commands.Reorder;

/// <summary>
/// Re-sequences a trip's stops. Sequences are re-normalized server-side so a client's ordering can
/// never violate the unique <c>(TripId, Sequence)</c> index, and the writer refuses to push a
/// completed stop below an uncompleted one — a reorder must not rewrite history.
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.TripManagement)]
// Enforcement: the handler derives the caller's own account and passes it to the reader/writer,
// which filters every row on it (TripVisibility is the single visibility resolver - spec 11).
[AccountScopeEnforcedInHandler]
public readonly record struct ReorderTripStopsCommand(Guid TripId, IReadOnlyCollection<Guid> OrderedStopIds) : IRequest;

public sealed class ReorderTripStopsCommandHandler(
    ITripStopWriter writer,
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<ReorderTripStopsCommand>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task Handle(ReorderTripStopsCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);
        var trip = await reader.GetTripAsync(request.TripId, caller.AccountId, scopeUserId, cancellationToken);

        if (TripStatuses.IsTerminal(trip.Status))
            throw TripValidationFailure.Create(nameof(ReorderTripStopsCommand.TripId), TripErrorCodes.TripAlreadyTerminal);

        await writer.ReorderStopsAsync(request.TripId, caller.AccountId, request.OrderedStopIds, cancellationToken);
    }
}

public sealed class ReorderTripStopsValidator : AbstractValidator<ReorderTripStopsCommand>
{
    public ReorderTripStopsValidator()
    {
        RuleFor(v => v.TripId).NotEmpty();
        RuleFor(v => v.OrderedStopIds).NotEmpty();
        RuleFor(v => v.OrderedStopIds)
            .Must(ids => ids is not null && ids.Distinct().Count() == ids.Count)
            .WithMessage("Ordered stop ids must be unique.");
    }
}
